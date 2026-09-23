@echo off
rem ============================================================
rem  Memetoglu Web - tek tikla guncelle ve calistir
rem  1) GitHub'daki son degisiklikleri indirir (git pull)
rem  2) 5080 portunda eski calisan surumu kapatir
rem  3) Siteyi baslatir ve tarayicida acar
rem  Kapatmak icin bu pencerede Ctrl+C veya pencereyi kapatin.
rem ============================================================
setlocal
cd /d "%~dp0"
title Memetoglu Web - http://localhost:5080

rem --- git'i bul: once PATH, yoksa Visual Studio'nun kendi git'i ---
set "GIT="
where git >nul 2>nul && set "GIT=git"
if defined GIT goto gitbulundu
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" goto gityok
for /f "usebackq delims=" %%G in (`call "%VSWHERE%" -latest -products * -find "**\Git\cmd\git.exe"`) do set "GIT=%%G"
if defined GIT goto gitbulundu

:gityok
echo.
echo  [!] Git bulunamadi, guncelleme atlaniyor. Git'i bir kez kurmak icin:
echo        winget install --id Git.Git -e
echo.
goto portkapat

:gitbulundu
if not exist ".git" goto gitdegil
echo.
echo  [1/3] GitHub'dan son degisiklikler aliniyor...
"%GIT%" pull --ff-only
if errorlevel 1 echo  [!] Guncelleme yapilamadi; mevcut surum calistirilacak.
goto portkapat

:gitdegil
echo.
echo  [!] Bu klasor git ile indirilmemis (ZIP). Guncelleme atlaniyor.

:portkapat
echo.
echo  [2/3] 5080 portundaki eski surum kapatiliyor...
powershell -NoProfile -Command "Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }"

echo.
echo  [3/3] Site baslatiliyor: http://localhost:5080
echo        (ilk derleme 20-30 saniye surebilir)
echo.
start "" /min powershell -NoProfile -WindowStyle Hidden -Command "for($i=0;$i -lt 90;$i++){try{Invoke-WebRequest -UseBasicParsing -TimeoutSec 2 http://localhost:5080/ | Out-Null; Start-Process 'http://localhost:5080'; break}catch{Start-Sleep 1}}"
dotnet run --launch-profile MemetogluWeb

echo.
pause
