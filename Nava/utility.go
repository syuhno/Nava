package main

import (
	"fmt"
	"path/filepath"
	"strings"
	"syscall"
	"time"
	"unsafe"

	"golang.org/x/sys/windows"
)

var (
	kernel32 = windows.NewLazySystemDLL("kernel32.dll")
	user32   = windows.NewLazySystemDLL("user32.dll")

	EnumWindows        = user32.NewProc("EnumWindows")
	ShowWindow         = user32.NewProc("ShowWindow")
	IsWindowVisible    = user32.NewProc("IsWindowVisible")
	GetWindowTextW     = user32.NewProc("GetWindowTextW")
	AllocConsole       = kernel32.NewProc("AllocConsole")
	GetModuleFileNameW = kernel32.NewProc("GetModuleFileNameW")
)

func getCurrentProcessName() string {
	var buf [windows.MAX_PATH]uint16
	r, _, _ := GetModuleFileNameW.Call(
		0,
		uintptr(unsafe.Pointer(&buf[0])),
		uintptr(len(buf)),
	)
	if r == 0 {
		return ""
	}

	fullPath := windows.UTF16ToString(buf[:r])
	return strings.ToLower(filepath.Base(fullPath))
}

func hideBanner() error {
	const bannerKeyword = "version"
	const maxRetries = 10
	const retryDelay = 100 * time.Millisecond

	for range maxRetries {
		var found bool

		cb := syscall.NewCallback(func(hwnd windows.Handle, _ uintptr) uintptr {
			if found {
				return 0
			}

			var buf [256]uint16
			n, _, _ := GetWindowTextW.Call(
				uintptr(hwnd),
				uintptr(unsafe.Pointer(&buf[0])),
				uintptr(len(buf)),
			)
			if n > 0 {
				title := strings.ToLower(windows.UTF16ToString(buf[:n]))
				if strings.Contains(title, bannerKeyword) {
					visible, _, _ := IsWindowVisible.Call(uintptr(hwnd))
					if visible != 0 {
						ShowWindow.Call(uintptr(hwnd), 0)
					}
					found = true
					return 0
				}
			}
			return 1
		})

		EnumWindows.Call(cb, 0)

		if found {
			return nil
		}
		time.Sleep(retryDelay)
	}

	return fmt.Errorf("[Nava::Error] banner window '%s' not found after retries", bannerKeyword)
}
