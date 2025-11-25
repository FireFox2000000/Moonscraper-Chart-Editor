#!/bin/bash

echo -e "\nThis will remove installed files for Moonscraper Chart Editor. You may be prompted for your password."

pkgname=moonscraper-chart-editor

sudo rm -Rf \
  "/opt/$pkgname" \
  "/usr/local/bin/$pkgname" \
  "/usr/share/applications/$pkgname.desktop" \
  "/usr/share/pixmaps/$pkgname"

echo -e "\nUninstall complete.\n"
