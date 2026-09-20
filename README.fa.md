# Raaya TypeSwitch

[English](README.md) | **فارسی**

**اصلاح هوشمند اشتباه تایپ فارسی ↔ انگلیسی در ویندوز**

Raaya TypeSwitch یک ابزار سبک برای ویندوز است که وقتی متن را با زبان اشتباه صفحه‌کلید تایپ می‌کنید، آن را تشخیص می‌دهد، متن قبلی را اصلاح می‌کند و Layout فعال را هم به زبان درست تغییر می‌دهد.

> **وضعیت پروژه:** نسخه MVP / آزمایشی. تمرکز فعلی روی فارسی و انگلیسی (US) است. اصلاح خودکار در پایان کلمه و با فشردن Space، Enter یا Tab انجام می‌شود.

## مثال

اگر صفحه‌کلید روی English باشد ولی بخواهید بنویسید:

```text
سلام
```

ممکن است این را تایپ کنید:

```text
sghl
```

Raaya TypeSwitch همان کلیدهای فیزیکی را با Layout فارسی بازسازی می‌کند و اگر اطمینان کافی داشته باشد:

```text
sghl → سلام
```

سپس Layout پنجره فعال را هم روی فارسی قرار می‌دهد.

همین روند در جهت معکوس هم انجام می‌شود. برای مثال:

```text
قششغش → raaya
یثحمخغ → deploy
```

## اصل مهم در تشخیص

در این پروژه **False Positive از False Negative بدتر است**.

یعنی اگر برنامه مطمئن نباشد، ترجیح می‌دهد هیچ تغییری ندهد تا اینکه یک کلمه درست را اشتباه اصلاح کند.

برای نمونه:

```text
مثلا
سلام
میخوام
API
JSON
raaya
deploy
```

نباید صرفاً به‌دلیل شباهت ظاهری به زبان دیگر تغییر کنند.

## قابلیت‌های فعلی

- تشخیص اشتباه Layout بین فارسی و انگلیسی
- اصلاح خودکار آخرین کلمه
- تعویض خودکار Layout پنجره فعال
- پردازش کاملاً محلی
- بدون نیاز به API یا سرویس ابری
- بدون ارسال متن تایپ‌شده به سرور
- اجرا به‌صورت Windows System Tray
- لاگ محلی برای خطاهای Startup
- الگوریتم محافظه‌کارانه برای کاهش False Positive
- ساختار توسعه‌پذیر برای زبان‌ها و Layoutهای بیشتر

## پیش‌نیازها

- Windows 10 یا Windows 11
- .NET 10 SDK برای Build از سورس
- نصب بودن Keyboard Layout انگلیسی (US)
- نصب بودن Keyboard Layout فارسی

## دریافت سورس

```powershell
git clone https://github.com/sam12r18/raaya-TypeSwitch.git
cd raaya-TypeSwitch
```

## Build

```powershell
dotnet restore
dotnet build RaayaTypeSwitch.slnx
```

## اجرای برنامه

```powershell
dotnet run --project .\src\RaayaTypeSwitch\RaayaTypeSwitch.csproj
```

بعد از اجرا، برنامه پنجره اصلی باز نمی‌کند و در **System Tray** ویندوز قرار می‌گیرد.

اگر آیکن را نمی‌بینید، بخش `^` کنار ساعت ویندوز را بررسی کنید.

## محل لاگ

اگر برنامه اجرا نشد یا بلافاصله بسته شد:

```text
%LOCALAPPDATA%\RaayaTypeSwitch\startup.log
```

نمونه مسیر:

```text
C:\Users\<username>\AppData\Local\RaayaTypeSwitch\startup.log
```

## ساخت نسخه مستقل EXE

برای ایجاد نسخه Self-contained و Single File برای Windows x64:

```powershell
dotnet publish .\src\RaayaTypeSwitch\RaayaTypeSwitch.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

خروجی:

```text
src\RaayaTypeSwitch\bin\Release\net10.0-windows\win-x64\publish\
```

## تست‌ها

بعد از Build:

```powershell
dotnet test RaayaTypeSwitch.slnx --configuration Release
```

تست‌های Regression برای جلوگیری از برگشت باگ‌های مهم استفاده می‌شوند. برای نمونه:

- `مثلا` نباید به `legh` تبدیل شود.
- `سلام` باید فارسی بماند.
- `raaya` و `deploy` نباید در حالت صحیح تغییر کنند.
- `قششغش` باید بتواند به `raaya` تبدیل شود.
- `یثحمخغ` باید بتواند به `deploy` تبدیل شود.
- `sghl` باید بتواند به `سلام` تبدیل شود.

## نحوه کار

Raaya TypeSwitch از یک Low-Level Keyboard Hook در ویندوز استفاده می‌کند.

برای هر کلمه:

1. کلیدهای فیزیکی تایپ‌شده را موقتاً در حافظه نگه می‌دارد.
2. Layout فعال پنجره را تشخیص می‌دهد.
3. همان کلیدها را با Layout دیگر بازسازی می‌کند.
4. دو خروجی را با Language Detector محلی مقایسه می‌کند.
5. فقط اگر اطمینان کافی وجود داشته باشد، متن را اصلاح می‌کند.
6. Layout پنجره فعال را نیز تغییر می‌دهد.

برای جلوگیری از Loop، رویدادهای صفحه‌کلیدی‌ای که خود برنامه Inject می‌کند دوباره پردازش نمی‌شوند.

جزئیات بیشتر در [مستند معماری](docs/ARCHITECTURE.md) آمده است.

## حریم خصوصی

طراحی پروژه Local-first است:

- متن تایپ‌شده به هیچ سروری ارسال نمی‌شود.
- هیچ سرویس AI ابری برای تشخیص لازم نیست.
- Buffer تایپ فعلی فقط در حافظه نگهداری می‌شود.
- پروژه وابستگی Runtime به CDN یا Asset خارجی ندارد.

### هشدار امنیتی نسخه فعلی

تشخیص Password / PIN / Secure Field هنوز کامل نشده است.

تا قبل از تکمیل این بخش، هنگام وارد کردن موارد حساس مثل:

- رمز عبور
- PIN
- Recovery Code
- API Key
- Token
- Private Key

برنامه را موقتاً غیرفعال کنید.

جزئیات در [SECURITY.md](SECURITY.md) آمده است.

## محدودیت‌های فعلی

- فقط فارسی و English (US) پشتیبانی می‌شوند.
- اصلاح خودکار فعلاً در پایان کلمه انجام می‌شود.
- Dictionary داخلی هنوز محدود است.
- Password Field Detection هنوز Release-ready نیست.
- Excluded Apps هنوز اضافه نشده است.
- بعضی برنامه‌های Run as Administrator ممکن است به سطح دسترسی مشابه نیاز داشته باشند.
- Remote Desktop، VMها، بازی‌ها یا برنامه‌هایی با سیستم ورودی اختصاصی ممکن است رفتار متفاوتی داشته باشند.

## نقشه راه

- [ ] تشخیص Password / PIN / Secure Field
- [ ] Excluded Applications
- [ ] Hotkey برای اصلاح دستی آخرین کلمه
- [ ] Undo آخرین اصلاح
- [ ] اجرای خودکار با Windows Startup
- [ ] Dictionary آفلاین بزرگ‌تر فارسی و انگلیسی
- [ ] N-gram Scoring محلی
- [ ] تنظیم حساسیت تشخیص
- [ ] تنظیمات مستقل برای هر برنامه
- [ ] UI تنظیمات
- [ ] تست‌های گسترده Keyboard Mapping
- [ ] Installer
- [ ] Signed Releases
- [ ] پشتیبانی از زبان‌های بیشتر

## ساختار پروژه

```text
RaayaTypeSwitch.slnx
src/
  RaayaTypeSwitch/
    Program.cs
    TrayApplicationContext.cs
    KeyboardHook.cs
    KeyboardLayoutService.cs
    TypingEngine.cs
    LanguageDetector.cs
    TextInjector.cs
    NativeMethods.cs

tests/
  RaayaTypeSwitch.Tests/

docs/
  ARCHITECTURE.md

.github/
  workflows/
    build.yml
```

## مشارکت

Issue، Bug Report، Test Case و Pull Request استقبال می‌شود.

قبل از مشارکت [CONTRIBUTING.md](CONTRIBUTING.md) را بخوانید.

در گزارش خطا بهتر است این موارد را بنویسید:

- نسخه ویندوز
- برنامه‌ای که داخل آن تایپ کرده‌اید
- Layout فعال
- متنی که قصد داشتید بنویسید
- متن اشتباه ایجادشده
- متن مورد انتظار

لطفاً هیچ رمز عبور، Token یا متن محرمانه‌ای را در Issue عمومی قرار ندهید.

## مجوز

Raaya TypeSwitch تحت مجوز [MIT](LICENSE) منتشر شده است.

---

یک ابزار متن‌باز از مجموعه Raaya برای کاهش اشتباه‌های رایج تایپ فارسی و انگلیسی در ویندوز.
