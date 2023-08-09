# TERA Arise

<div align="center">
    <img src="arise.png"
         width="128" />
</div>

<div align="center">

[![License](https://img.shields.io/github/license/tera-arise/arise?color=brown)](LICENSE-AGPL-3.0)
[![Commits](https://img.shields.io/github/commit-activity/m/tera-arise/arise/master?label=commits&color=slateblue)](https://github.com/tera-arise/arise/commits/master)
[![Build](https://img.shields.io/github/actions/workflow/status/tera-arise/arise/build.yml?branch=master)](https://github.com/tera-arise/arise/actions/workflows/build.yml)
[![Discussions](https://img.shields.io/github/discussions/tera-arise/arise?color=teal)](https://github.com/tera-arise/arise/discussions)
[![Discord](https://img.shields.io/discord/1049553965987143750?color=peru&label=discord)](https://discord.gg/BZnmVMGYa9)

</div>

--------------------------------------------------------------------------------

**TERA Arise** is a game resurrection project for the final build of
[TERA](https://en.wikipedia.org/wiki/TERA_(video_game)) EU, 115.02 (r387486),
which was released a couple of months prior to the game's official shutdown on
June 30, 2022.

## Philosophy

Unlike typical server emulation projects, TERA Arise is not trying to replicate
the original game. Rather, we are using the game client and assets as a base to
build upon, with the goal of eventually producing a better experience than the
original game. We will essentially start from scratch, with an empty data center
file, and work our way from there.

Once the project progresses far enough, our goal is to run official TERA Arise
servers at <https://tera-arise.io>. That said, anyone is welcome to run their
own public servers, as long as the [licensing terms](#licensing) are observed.

## Usage

Building and running TERA Arise requires the following:

* Windows 10+ or Ubuntu 23.04+
* .NET SDK 8.0.0 Preview 6
* PostgreSQL 14.6+

Simply run `./cake` to build client artifacts for Windows (x64) and server
artifacts for the current platform (Windows or Linux, x64 or Arm64).

By default, `./cake` will build in the `Debug` configuration which is suitable
for local development. The `Release` configuration (i.e. `./cake -c Release`) is
used for staging and production scenarios.

You will need to set up one or more databases to run the server daemon. Below is
one possible way to do it which will match the expectations of the default
configuration files.

Create a user called `arise`:

```sql
CREATE USER arise PASSWORD 'arise';
```

Create a database called `arise`, owned by `arise`:

```sql
-- PostgreSQL 14
CREATE DATABASE arise OWNER 'arise' TEMPLATE 'template0' ENCODING 'utf8' LOCALE 'C';
-- PostgreSQL 15
CREATE DATABASE arise OWNER 'arise' TEMPLATE 'template0' ENCODING 'utf8' LOCALE 'und-x-icu' ICU_LOCALE 'und' LOCALE_PROVIDER 'icu';
```

Finally, while connected to the `arise` database, create schemas for each
deployment environment:

```sql
CREATE SCHEMA development AUTHORIZATION arise;
CREATE SCHEMA production AUTHORIZATION arise;
CREATE SCHEMA staging AUTHORIZATION arise;
```

With this setup, and assuming you have PostgreSQL listening locally, you should
now be able to successfully do `dotnet run --project src/server/daemon`.

## Licensing

Server emulation for online games has traditionally suffered from bad actors
taking and modifying the open source code, running for-profit servers, and then
contributing nothing back to the project. To combat this, TERA Arise is licensed
under the [GNU Affero General Public License 3.0](LICENSE-AGPL-3.0). This means
that anyone running TERA Arise *must* provide the full source code of the client
and server components to users.

To be clear, this license only applies to TERA Arise itself; the original TERA
game client and its assets are not subject to it.
