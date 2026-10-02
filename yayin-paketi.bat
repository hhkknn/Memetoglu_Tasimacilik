@echo off
rem ============================================================
rem  Memetoglu Web - yayin paketi hazirla
rem  "yayin" klasorune iki ZIP uretir:
rem    ilk-kurulum.zip  : sunucuya ILK yuklemede kullanin (her sey dahil)
rem    guncelleme.zip   : sonraki guncellemelerde kullanin. App_Data icermez;
rem                       panelden yaptiginiz degisikliklerin ustune yazmaz.
rem ============================================================
setlocal
cd /d "%~dp0"
title Memetoglu Web - yayin paketi

rem --- Once GitHub'daki son surumu al: eski kopya paketlenmesin ---
set "GIT="
where git >nul 2>nul && set "GIT=git"
if defined GIT goto gitbulundu
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" goto gitatla
for /f "usebackq delims=" %%G in (`call "%VSWHERE%" -latest -products * -find "**\Git\cmd\git.exe"`) do set "GIT=%%G"
if not defined GIT goto gitatla

:gitbulundu
if not exist ".git" goto gitatla
echo.
echo  [0/3] GitHub'dan son degisiklikler aliniyor...
"%GIT%" pull --ff-only
if not errorlevel 1 goto gitbitti
echo.
echo  [!] Son surum ALINAMADI. Paket eski surumle hazirlanirdi, durduruldu.
echo      Asagida bu bilgisayarda degistirilmis dosyalar listelenir:
"%GIT%" status --short
echo.
echo      App_Data\content.json listedeyse yerel panelde yapilan degisiklik
echo      guncellemeyi engelliyordur. Yerel degisikligi atmak icin:
echo        "%GIT%" checkout -- App_Data/content.json
echo      komutunu calistirip bu dosyayi tekrar acin.
echo.
pause
exit /b 1

:gitatla
echo.
echo  [!] Git bulunamadi; GitHub'dan guncelleme alinmadan paketleniyor.

:gitbitti
if defined GIT (
  echo      Paketlenen surum:
  "%GIT%" log -1 --oneline
)

echo.
echo  [1/3] Yayin surumu derleniyor...
if exist publish rmdir /s /q publish
dotnet publish -c Release -o publish
if errorlevel 1 goto hata

echo.
echo  [2/3] Yerel test dosyalari temizleniyor...
powershell -NoProfile -Command "Get-ChildItem 'publish\wwwroot\uploads' -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne '.gitkeep' } | Remove-Item -Recurse -Force"

echo.
echo  [3/3] ZIP paketleri olusturuluyor...
if not exist yayin mkdir yayin
if exist yayin\ilk-kurulum.zip del /q yayin\ilk-kurulum.zip
if exist yayin\guncelleme.zip del /q yayin\guncelleme.zip
powershell -NoProfile -Command "Compress-Archive -Path 'publish\*' -DestinationPath 'yayin\ilk-kurulum.zip' -Force"
if errorlevel 1 goto hata
if exist "%TEMP%\memetoglu-guncelleme" rmdir /s /q "%TEMP%\memetoglu-guncelleme"
xcopy publish "%TEMP%\memetoglu-guncelleme\" /e /i /q >nul
rmdir /s /q "%TEMP%\memetoglu-guncelleme\App_Data"
powershell -NoProfile -Command "Compress-Archive -Path '%TEMP%\memetoglu-guncelleme\*' -DestinationPath 'yayin\guncelleme.zip' -Force"
if errorlevel 1 goto hata
rmdir /s /q "%TEMP%\memetoglu-guncelleme"

echo.
echo  Hazir. "yayin" klasoru aciliyor:
echo    ilk-kurulum.zip  - sunucuya ilk yukleme
echo    guncelleme.zip   - sonraki guncellemeler
echo.
start "" "%~dp0yayin"
pause
exit /b 0

:hata
echo.
echo  [!] Bir hata olustu. Yukaridaki mesajin ekran goruntusunu gonderin.
echo.
pause
exit /b 1
