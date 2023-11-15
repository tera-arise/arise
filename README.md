# TERA Arise

<div align="center">
    <img src="arise.png"
         width="128" />
</div>

<div align="center">

[![License](https://img.shields.io/github/license/tera-arise/arise?color=brown)](LICENSE-AGPL-3.0)
[![Commits](https://img.shields.io/github/commit-activity/m/tera-arise/arise/master?label=commits&color=slateblue)](https://github.com/tera-arise/arise/commits/master)
[![Build](https://img.shields.io/github/actions/workflow/status/tera-arise/arise/build.yml?branch=master)](https://github.com/tera-arise/arise/actions/workflows/build.yml)
[![Discord](https://img.shields.io/discord/1049553965987143750?color=peru&label=discord)](https://discord.gg/BZnmVMGYa9)

</div>

--------------------------------------------------------------------------------

**TERA Arise** is a game resurrection project for the final build of
[TERA](https://en.wikipedia.org/wiki/TERA_(video_game)) EU, 115.02 (r387486),
which was released a couple of months prior to the game's official shutdown on
June 30, 2022.

This project is still in heavy development.

## Philosophy

Unlike typical server emulation projects, TERA Arise is not trying to replicate
the original game. Rather, we are using the game client and assets as a base to
build upon, with the goal of eventually producing a better experience than the
original game. We will essentially start from scratch, with an empty data center
file, and work our way from there. Our overarching design goals are laid out on
our [GitBook space](https://design.tera-arise.io).

Once the project progresses far enough, our goal is to run official TERA Arise
servers at <https://tera-arise.io>. That said, anyone is welcome to run their
own public servers, as long as the [licensing terms](#licensing) are observed.

## Usage

Building and running TERA Arise requires the following:

* Windows 11+ or Ubuntu 23.04+
* .NET SDK 8.0.0
* PostgreSQL 16.0+

Development will typically require one of the following editors:

* Visual Studio 2022 17.7.6+
* Visual Studio Code 1.84.2+
* JetBrains Rider 2023.2+
* JetBrains Fleet 1.24+

Simply run `./cake` (a [Bash](https://www.gnu.org/software/bash) script) to
build client artifacts for Windows (x64) and server artifacts for the current
platform (Windows or Linux, x64 or Arm64). This will use the `Debug`
configuration by default, which is suitable for development and debugging. Pass
`-c Release` instead to use that configuration, resulting in an optimized build.

Note that `Debug` and `Release` configurations are orthogonal to whether the
resulting artifacts are suitable for deployment; see the
[vendoring instructions](#vendoring).

You will need to set up one or more databases to run the server daemon. Below is
one possible way to do it which will match the expectations of the default
configuration files.

Create a user called `arise`:

```sql
CREATE USER arise PASSWORD 'arise';
```

Create a database called `arise`, owned by `arise`:

```sql
CREATE DATABASE arise OWNER 'arise' TEMPLATE 'template0' ENCODING 'utf8' LOCALE 'und-x-icu' ICU_LOCALE 'und' LOCALE_PROVIDER 'icu';
```

Finally, while connected to the `arise` database, create schemas for each kind
of environment:

```sql
CREATE SCHEMA development AUTHORIZATION arise;
CREATE SCHEMA production AUTHORIZATION arise;
CREATE SCHEMA staging AUTHORIZATION arise;
```

(The latter two are only required for non-development deployments.)

With this setup, and assuming you have PostgreSQL listening locally, you should
now be able to successfully do `dotnet run --project src/server/daemon`.

## Vendoring

In order to deploy TERA Arise in non-development scenarios, it is necessary to
set up a so-called vendor project. This project provides network certificates,
data center parameters, and more. A default vendor project intended for
development purposes is included in the [`vnd`](vnd) directory and serves as a
base for customization.

The vendor project path can be overridden by passing `--vendor <path>` to
`./cake`. The recommended setup is to have a repository with the TERA Arise
repository as a submodule, alongside a custom vendor project. A small build
script can then invoke something like
`arise/cake -c Release --vendor vendor/vendor.proj`.

Additionally, for the server daemon, an extra configuration file is required,
named either `arised.staging.json` or `arised.production.json` depending on
environment. The following is an example of a bare-bones production
configuration:

```json
{
  "Storage": {
    "ConnectionString": "Host=storage.my-tera.com; Username=arise; Password=arise; Database=production; Root Certificate=postgresql.pem; SSL Mode=VerifyCA"
  },
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "web.pem",
        "KeyPath": "web.key"
      }
    },
    "Endpoints": {
      "Web": {
        "Url": "https://my-tera.com:443"
      }
    }
  },
  "Web": {
    "EmailAddress": "system@my-tera.com",
    "SendGridKey": "<SendGrid API key>"
  },
  "World": {
    "Endpoints": [
      "0.0.0.0:7801",
      ":::7801"
    ]
  }
}
```

## Licensing

Server emulation for online games has traditionally suffered from bad actors
taking and modifying the open source code, running for-profit servers, and then
contributing nothing back to the project. To combat this, TERA Arise is licensed
under the [GNU Affero General Public License 3.0](LICENSE-AGPL-3.0). This means
that anyone running TERA Arise *must* provide the full source code of the client
and server components to users.

To be clear, this license only applies to TERA Arise itself; the original TERA
game client and its assets are not subject to it.
