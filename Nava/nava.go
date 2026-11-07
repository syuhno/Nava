package main

import "C"
import (
	"fmt"
	"os"
	"path/filepath"
	"syscall"
	"time"
	"unsafe"

	"github.com/ropnop/go-clr"
)

func checkOK(hr uintptr, caller string) {
	if hr != 0x0 {
		fmt.Printf("[Nava::CLR] %s returned 0x%08x\n", caller, hr)
	}
}

func AllocTerm() {
	AllocConsole.Call()

	stdout, _ := syscall.Open("CONOUT$", syscall.O_RDWR, 0)
	if stdout != syscall.InvalidHandle {
		os.Stdout = os.NewFile(uintptr(stdout), "/dev/stdout")
		os.Stderr = os.NewFile(uintptr(stdout), "/dev/stderr")
	}
	fmt.Println("[Nava] Nava payload loaded successfully")
}

func init() {
	time.Sleep(300 * time.Millisecond)

	metaHost, err := clr.GetICLRMetaHost()
	if err != nil {
		fmt.Println("[Nava::Error] Failed to create CLR metahost:", err)
		return
	}
	defer metaHost.Release()

	versionString := "v4.0.30319"
	var pRuntimeInfo uintptr

	pwzVersion, _ := syscall.UTF16PtrFromString(versionString)
	hr := metaHost.GetRuntime(pwzVersion, &clr.IID_ICLRRuntimeInfo, &pRuntimeInfo)
	checkOK(hr, "metaHost.GetRuntime")

	runtimeInfo := clr.NewICLRRuntimeInfoFromPtr(pRuntimeInfo)
	if runtimeInfo == nil {
		fmt.Println("[Nava::Error] runtimeInfo is nil")
		return
	}

	var pRuntimeHost uintptr
	hr = runtimeInfo.GetInterface(&clr.CLSID_CLRRuntimeHost, &clr.IID_ICLRRuntimeHost, &pRuntimeHost)
	checkOK(hr, "runtimeInfo.GetInterface")

	host := clr.NewICLRRuntimeHostFromPtr(pRuntimeHost)

	mDll := filepath.Join(os.TempDir(), "milim.dll")

	pDLLPath, _ := syscall.UTF16PtrFromString(mDll)
	pTypeName, _ := syscall.UTF16PtrFromString("Milim.InitMilim")
	pMethodName, _ := syscall.UTF16PtrFromString("Nava")
	pArgument, _ := syscall.UTF16PtrFromString("unused")

	var returnVal uint32
	hr = host.ExecuteInDefaultAppDomain(
		pDLLPath,
		pTypeName,
		pMethodName,
		pArgument,
		(*uint16)(unsafe.Pointer(&returnVal)),
	)
	if hr != 0 {
		fmt.Printf("[Nava::Error] ExecuteInDefaultAppDomain failed (code: 0x%x)\n", hr)
		return
	}

	if getCurrentProcessName() == "safeexambrowser.client.exe" {
		if err := hideBanner(); err != nil {
			fmt.Println(err.Error())
		}
	}
}

func main() {}
