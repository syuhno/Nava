package main

import (
	"fmt"
	"os"
	"os/signal"
	"syscall"
	"time"
)

func main() {
	sebParent := "safeexambrowser.exe"
	sebClient := "safeexambrowser.client"

	fmt.Println("[Nava] Running setup")
	nDll := setup()
	if nDll == "" {
		fmt.Println("[Nava::Error] Setup failed, unable to extract required libraries")
		return
	}
	enablePriv()

	fmt.Println("[Nava] Ready . . .")
	sebPid := findProcessSync(sebParent)

	if sebPid != 0 {
		fmt.Println("[Nava] Found parent pid")

		time.Sleep(100 * time.Millisecond)
		if err := inject(sebPid, nDll); err != nil {
			fmt.Println("[Nava::Error] Failed to inject nava in parent process:", err.Error())
		} else {
			markInjected(sebPid)
		}
	}

	fmt.Println("[Nava::AutoReInject] Starting watcher")

	go backgroundInject(
		sebClient,
		nDll,
		500*time.Millisecond,
	)

	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, os.Interrupt, syscall.SIGTERM)
	<-sigChan
}
