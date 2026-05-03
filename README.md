# VLESS VPN Client

Полнофункциональный VPN-клиент для **Windows** под протокол **VLESS** (xray-core),
аналог мобильного приложения **Happ**, но для ПК. Написан на C# (WPF, .NET 8).

## Возможности

- Импорт серверов через `vless://` URL и подписки (plain text + base64)
- Поддержка форматов транспорта: TCP, WebSocket, gRPC, HTTP/2, mKCP, QUIC
- Поддержка `security`: none, **TLS**, **REALITY** (с pbk/sid/spx/fingerprint)
- Поддержка `flow=xtls-rprx-vision`
- Запуск/остановка xray-core как дочернего процесса с авто-генерацией `config.json`
- Системный прокси Windows (HTTP/SOCKS) с обходом локальных адресов
- TCP-пинг всех серверов с цветовой индикацией и сортировкой
- Менеджер подписок с автообновлением
- Tray-иконка с быстрым подключением/отключением
- Автозапуск с Windows и сворачивание в трей
- Тёмная и светлая тема
- Просмотр логов и хранение настроек в `%LocalAppData%\VlessVpnClient`
- Установщик Inno Setup, который скачивает свежий xray-core при установке

## Скриншоты

Будут добавлены после первого UI-теста.

## Архитектура

```
VlessVpnClient.sln
├── src/
│   ├── VlessVpnClient.Core/      # бизнес-логика (.NET 8, кросс-платформа)
│   │   ├── Models/               # VlessConfig, ServerProfile, AppSettings
│   │   ├── Parsing/              # VLESS URL parser, subscription parser
│   │   ├── Xray/                 # config.json generator, process manager, downloader
│   │   ├── Network/              # ping tester, system proxy
│   │   ├── Storage/              # JSON persistence для профилей/настроек
│   │   ├── Subscriptions/        # subscription updater
│   │   └── Logging/              # FileLogger
│   └── VlessVpnClient.App/       # WPF UI (.NET 8 windows)
│       ├── App.xaml              # ресурсы, темы
│       ├── MainWindow.xaml       # главное окно
│       ├── Views/                # ImportDialog, SettingsWindow, LogsWindow, SubscriptionsWindow
│       ├── ViewModels/           # MVVM
│       ├── Services/             # ConnectionManager, ThemeManager, AutoStartManager
│       ├── Themes/               # DarkTheme, LightTheme, Styles
│       ├── Converters/           # XAML-конвертеры
│       └── Helpers/               # ObservableObject, RelayCommand
├── tests/
│   └── VlessVpnClient.Core.Tests/
├── installer/
│   ├── setup.iss                 # Inno Setup script
│   └── download-xray.ps1         # PowerShell для скачивания xray-core
└── .github/workflows/build.yml   # CI на windows-latest
```

## Сборка локально

Требуется Windows + .NET 8 SDK.

```powershell
dotnet restore VlessVpnClient.sln
dotnet build VlessVpnClient.sln -c Release
dotnet test tests/VlessVpnClient.Core.Tests
dotnet publish src/VlessVpnClient.App/VlessVpnClient.App.csproj `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -o publish/VlessVpnClient
```

## Сборка инсталлятора

Установите [Inno Setup 6](https://jrsoftware.org/isdl.php) и запустите:

```powershell
& 'C:\Program Files (x86)\Inno Setup 6\ISCC.exe' installer\setup.iss
```

Готовый `VlessVpnClient-Setup-1.0.0.exe` появится в `installer\Output\`.

## Где xray-core?

xray-core НЕ коммитится в репозиторий. Он скачивается автоматически:

1. Инсталлятором Inno Setup при установке (через `download-xray.ps1`)
2. Самим приложением при первом подключении, если бинарник не найден (через `XrayDownloader`)

Источник: <https://github.com/XTLS/Xray-core/releases/latest>

## CI

GitHub Actions собирает приложение и инсталлятор на каждом push/pull request.
Артефакты:

- `vless-vpn-client-app-win-x64` — single-file `.exe`
- `vless-vpn-client-installer` — `VlessVpnClient-Setup-X.Y.Z.exe`

## Лицензия

MIT — см. [LICENSE](LICENSE).
