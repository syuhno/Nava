package main

import (
	_ "embed"
	"fmt"
	"strings"
	"sync"
	"syscall"
	"time"
	"unsafe"

	"github.com/gentlemanautomaton/winproc/psapi"
	"golang.org/x/sys/windows"
)

//go:embed Milim.dll
var milimDll []byte

//go:embed nava.dll
var navaDll []byte

var (
	kernel32 = windows.NewLazySystemDLL("kernel32.dll")

	procVirtualAllocEx     = kernel32.NewProc("VirtualAllocEx")
	procVirtualFreeEx      = kernel32.NewProc("VirtualFreeEx")
	procCreateRemoteThread = kernel32.NewProc("CreateRemoteThread")
	procLoadLibraryW       = kernel32.NewProc("LoadLibraryW")

	injectedPIDs = make(map[uint32]bool)
	mInjected    sync.RWMutex
)

func findProcessSync(name string) uint32 {
	targetLower := strings.ToLower(name)
	for {
		pids, err := findProcessesByName(targetLower)
		if err == nil && len(pids) > 0 {
			return pids[0]
		}
		time.Sleep(200 * time.Millisecond)
	}
}

func backgroundInject(targetName, dllPath string, interval time.Duration) {
	fmt.Printf("[Nava] Watching for %s . . . \n", targetName)

	for {
		pids, err := findProcessesByName(targetName)
		if err != nil {
			fmt.Printf("[Nava::Warn] Scan failed: %v\n", err)
			time.Sleep(interval)
			continue
		}

		for _, pid := range pids {
			if !isInjected(pid) {
				fmt.Printf("[Nava] Detected %s (PID: %d), injecting...\n", targetName, pid)

				if err := inject(pid, dllPath); err != nil {
					fmt.Printf("[Nava::Error] Inject failed for PID %d: %v\n", pid, err)
					continue
				}

				markInjected(pid)
				fmt.Printf("[Nava] Injected into PID %d\n", pid)
			}
		}

		cleanDeadProcesses(pids)
		time.Sleep(interval)
	}
}

func findProcessesByName(targetName string) ([]uint32, error) {
	var pids []uint32
	targetLower := strings.ToLower(targetName)

	snapshot, err := psapi.CreateSnapshot(psapi.SnapAll, 0)
	if err != nil {
		return nil, err
	}
	defer syscall.CloseHandle(snapshot)

	proc, err := psapi.FirstProcess(snapshot)
	for err == nil {
		if strings.Contains(strings.ToLower(proc.Name()), targetLower) {
			pids = append(pids, proc.ProcessID)
		}
		proc, err = psapi.NextProcess(snapshot)
	}
	return pids, nil
}

func cleanDeadProcesses(currentPIDs []uint32) {
	mInjected.Lock()
	defer mInjected.Unlock()

	currentSet := make(map[uint32]struct{}, len(currentPIDs))
	for _, pid := range currentPIDs {
		currentSet[pid] = struct{}{}
	}

	for pid := range injectedPIDs {
		if _, exists := currentSet[pid]; !exists {
			fmt.Printf("[Nava] Process %d exited, removed from tracking\n", pid)
			delete(injectedPIDs, pid)
		}
	}
}

func isInjected(pid uint32) bool {
	mInjected.RLock()
	defer mInjected.RUnlock()
	return injectedPIDs[pid]
}

func markInjected(pid uint32) {
	mInjected.Lock()
	defer mInjected.Unlock()
	injectedPIDs[pid] = true
}

func inject(pid uint32, dllpath string) error {
	hProc, err := windows.OpenProcess(
		windows.PROCESS_VM_OPERATION|
			windows.PROCESS_VM_WRITE|
			windows.PROCESS_CREATE_THREAD|
			windows.PROCESS_QUERY_INFORMATION,
		false,
		pid,
	)
	if err != nil {
		return err
	}
	defer syscall.CloseHandle(syscall.Handle(hProc))

	loadLibAddr := procLoadLibraryW.Addr()
	if loadLibAddr == 0 {
		return fmt.Errorf("failed to get proc address for LoadLibraryW")
	}

	dllPathUtf16, err := windows.UTF16FromString(dllpath)
	if err != nil {
		return err
	}
	byteLen := uintptr(len(dllPathUtf16) * 2)

	addr, _, _ := procVirtualAllocEx.Call(
		uintptr(hProc),
		0,
		byteLen,
		windows.MEM_COMMIT|windows.MEM_RESERVE,
		windows.PAGE_READWRITE,
	)
	if addr == 0 {
		return windows.GetLastError()
	}
	defer procVirtualFreeEx.Call(uintptr(hProc), addr, 0, windows.MEM_RELEASE)

	var bytesWritten uintptr
	err = windows.WriteProcessMemory(hProc, addr, (*byte)(unsafe.Pointer(&dllPathUtf16[0])), byteLen, &bytesWritten)
	if err != nil {
		return err
	}

	hThread, _, _ := procCreateRemoteThread.Call(uintptr(hProc), 0, 0, loadLibAddr, addr, 0, 0)
	if hThread == 0 {
		return windows.GetLastError()
	}
	defer syscall.CloseHandle(syscall.Handle(hThread))

	syscall.WaitForSingleObject(syscall.Handle(hThread), windows.INFINITE)
	return nil
}

func enablePriv() {
	var hToken syscall.Token
	var luid windows.LUID
	currProc, _ := syscall.GetCurrentProcess()

	err := syscall.OpenProcessToken(currProc, syscall.TOKEN_ADJUST_PRIVILEGES|syscall.TOKEN_QUERY, &hToken)
	if err != nil {
		fmt.Println("[Nava::Error] OpenProcessToken failed:", err)
		return
	}
	defer syscall.CloseHandle(syscall.Handle(hToken))

	if err = windows.LookupPrivilegeValue(nil, windows.StringToUTF16Ptr("SeDebugPrivilege"), &luid); err != nil {
		fmt.Println("[Nava::Error] LookupPrivilege failed:", err)
		return
	}

	tokenPriv := windows.Tokenprivileges{
		PrivilegeCount: 1,
		Privileges: [1]windows.LUIDAndAttributes{
			{
				Luid:       luid,
				Attributes: windows.SE_PRIVILEGE_ENABLED,
			},
		},
	}

	privSize := uint32(unsafe.Sizeof(tokenPriv))
	if err = windows.AdjustTokenPrivileges(windows.Token(hToken), false, &tokenPriv, privSize, nil, nil); err != nil {
		fmt.Println("[Nava::Error] AdjustTokenPrivileges failed:", err)
		return
	}

	if lastErr, ok := windows.GetLastError().(syscall.Errno); ok && lastErr == windows.ERROR_NOT_ALL_ASSIGNED {
		fmt.Println("[Nava::Error] Not all privileges assigned successfully")
		return
	}

	fmt.Println("[Nava] Success enable privilege")
}
