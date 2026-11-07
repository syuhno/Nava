package main

import (
	"bytes"
	"fmt"
	"os"
	"path/filepath"
)

func setup() string {
	temp := os.TempDir()
	nDllPath := filepath.Join(temp, "nava.dll")
	mDllPath := filepath.Join(temp, "milim.dll")

	fmt.Printf("[Nava] Extracting %s\n", nDllPath)
	if err := extractDll(navaDll, nDllPath); err != nil {
		fmt.Printf("[Nava::Error] Failed to extract nava: %v\n", err)
		return ""
	}

	fmt.Printf("[Nava] Extracting %s\n", mDllPath)
	if err := extractDll(milimDll, mDllPath); err != nil {
		fmt.Printf("[Nava::Error] Failed to extract milim: %v\n", err)
		return ""
	}

	return nDllPath
}

func extractDll(data []byte, destPath string) error {
	if dir := filepath.Dir(destPath); dir != "." && dir != "" {
		if err := os.MkdirAll(dir, 0755); err != nil {
			return err
		}
	}

	if existingData, err := os.ReadFile(destPath); err == nil {
		if bytes.Equal(existingData, data) {
			return nil
		}
	}

	return os.WriteFile(destPath, data, 0755)
}
