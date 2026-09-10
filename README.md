<div align="center">
  <a href="https://github.com/aptabase/aptabase">
    <img src="https://aptabase.com/og.png" alt="Aptabase"/>
  </a>

  <h3 align="center">Aptabase</h3>

  <p align="center">
    Analytics for Apps.
    <br />
    Open Source. Privacy-First. Simple.
    <br />
    <br />
    <a href="https://aptabase.com"><strong>Learn more »</strong></a>
  </p>
</div>

> **This is `aptabase-plus`**, a downstream distribution of
> [aptabase/aptabase](https://github.com/aptabase/aptabase) maintained by
> [@nmehlei](https://github.com/nmehlei). It tracks upstream `main` and
> adds a stable, API-key-authenticated management API
> (`/api/v0/apps`, `/api/v0/api-keys`) so tools like
> [terraform-provider-aptabase](https://github.com/nmehlei/terraform-provider-aptabase)
> can manage apps without a browser session. Not officially affiliated
> with Aptabase. See `docs/upstream-proposal.md` for the design and its
> upstream discussion status.

# Management API (aptabase-plus addition)

This fork adds an API-key-authenticated management API so non-interactive
tooling (CI, Terraform, scripts) can manage apps without a browser session.

## Claimed routes

`/api/v0/` is **not** a greenfield namespace — upstream already serves
routes under it (`/api/v0/event`, `/api/v0/events`, `/api/v0/feature-flags/*`,
and `/api/v0/apps/{appId}/errors[...]`). This fork **extends** that existing,
upstream-owned prefix with:

| Method | Path |
| --- | --- |
| `GET`, `POST` | `/api/v0/apps` |
| `GET`, `PUT`, `DELETE` | `/api/v0/apps/{appId}` |
| `GET` | `/api/v0/apps/{appId}/shares` |
| `PUT`, `DELETE` | `/api/v0/apps/{appId}/shares/{email}` |
| `GET`, `POST` | `/api/v0/api-keys` |
| `DELETE` | `/api/v0/api-keys/{keyId}` |

**Known risk:** because `/api/v0/` is shared with upstream, a future upstream
change under that prefix could collide with these routes on rebase. The
fork accepts this and pins the paths here so collisions are caught early.

## API keys grant FULL account access

An `aptb_`-prefixed bearer key inherits **all** of its owning user's
permissions with no way to scope it down. It works on every authenticated
endpoint — that explicitly **includes deleting the account
(`POST /api/_auth/account/delete`) and the billing endpoints**, not just
app/share management. Treat a key exactly like the account password.
Restricting keys to a subset of routes or to the cookie scheme is
deliberately out of scope for v1.

# About the Project

[Aptabase](https://aptabase.com) is an open-source alternative to Firebase/Google Analytics, specifically built for Mobile, Desktop and Web apps.

📱 **Extensive list of SDK**: No matter what framework or language you use, we have an SDK for you. Swift, React Native, Flutter, Electron, Kotlin, and many others.

😇 **Privacy-First**: We prioritize user privacy and collect minimal usage data without using unique identifiers. Instead, we focus on monitoring sessions, complying fully with GDPR, CCPA, and PECR regulations.

🚀 **Simple**: Built-in and user-friendly dashboard for all your essential metrics, enabling you to gain insights effortlessly and grasp the dynamics of your apps.

💯 **Open-Source**: Our source code is 100% open source. There is nothing hidden. All the server code and SDKs are available for you to inspect and contribute to.

> [!TIP]
> Like what you see? Don't forget to give us a ⭐️ on GitHub!

# How to get started?

## ☁️ Managed Cloud

The easiest and quickest way to get started. A fully managed service by the creators of Aptabase to help you get started in minutes. Forget about managing software updates and patches; we do it all for you!

[Try free now →](https://aptabase.com)

## 🏢 Self-Hosted

You can also host Aptabase on your servers. It's free, but you are responsible for maintenance and updates.

[Learn how →](https://github.com/aptabase/self-hosting).

## 🛠️ Contributing

We welcome contributions from everyone! If you want to contribute to Aptabase, please read the guide below to get started.

[Contributing →](./CONTRIBUTING.md).

# SDKs

We provide SDKs for the most popular frameworks and languages to make it easier to integrate them into your app.

- [Swift (Apple)](https://github.com/aptabase/aptabase-swift)
- [Android (Kotlin)](https://github.com/aptabase/aptabase-kotlin)
- [React Native](https://github.com/aptabase/aptabase-react-native)
- [Flutter](https://github.com/aptabase/aptabase_flutter)
- [Tauri](https://github.com/aptabase/tauri-plugin-aptabase)
- [NativeScript](https://github.com/nstudio/nativescript-plugins/tree/main/packages/nativescript-aptabase)
- [.NET MAUI](https://github.com/aptabase/aptabase-maui)
- [Electron](https://github.com/aptabase/aptabase-electron)
- [Web Apps](https://github.com/aptabase/aptabase-js)
- [Unreal Engine](https://github.com/aptabase/aptabase-unreal)
- [Unity Engine](https://github.com/aptabase/aptabase-unity)

🛠️ Don't see the SDK for your platform? You can build your own! Check out our guide on [how to build your own SDK](https://github.com/aptabase/aptabase/wiki/How-to-build-your-own-SDK).

# Need help?

- Find us on [Discord](https://discord.gg/d9d97unCUk) or [Twitter](https://twitter.com/aptabase)
- Open a GitHub issue or a discussion on this repo
- Shoot us an email [hi@aptabase.com](mailto:hi@aptabase.com)

# Credits

Aptabase is highly inspired by [Plausible](https://github.com/plausible/analytics). If you're looking for privacy-friendly website analytics, go check them out. They're awesome! ❤️

# License

Aptabase is open-source under the [AGPLv3 license](./LICENSE). You can use it for free, but you must share any changes you make to the code.

The SDKs are open-source under the MIT license. Use it on your apps without any restrictions.
