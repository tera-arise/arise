# TERA Arise

TERA Arise is a game resurrection project for the final build of TERA EU.

## Confidentiality

This is currently a private project. Until that changes, under no circumstances
should code, documentation, assets, etc be shared with anyone outside the
development team.

An obvious exception exists for binary artifacts that need to be distributed to
clients. Broadly speaking, this includes most things in
[`src/shared`](src/shared) and [`src/client`](src/client).

## Authentication

All contributors must use
[SSH commit signing](https://docs.github.com/en/authentication/managing-commit-signature-verification/about-commit-signature-verification#ssh-commit-signature-verification).

To set up Git to verify signatures in Git history, add something like this to
your `~/.gitconfig`:

```gitconfig
[includeif "gitdir/i:~/tera-arise/**"]
    path = ~/tera-arise/.gitconfig
```

Then add this to `~/tera-arise/.gitconfig`:

```gitconfig
[gpg.ssh]
    allowedSignersFile = ../arise/allowed_signers
```

(This setup assumes that you have the various TERA Arise repositories cloned in
the `~/tera-arise` directory.)

## Building

Building and running TERA Arise requires the following:

* .NET SDK 8.0.0 Preview 6
* GnuTLS 3.7.8+
* PostgreSQL 14.6+

Simply run `./cake` to build client and server artifacts.

By default, `./cake` will build in the `Debug` configuration which is suitable
for local development. The `Release` configuration (i.e. `./cake -c Release`) is
used for staging and production deployments.

## Database

You will need to set up one or more databases to run TERA Arise. This section
explains one possible way to do it which will match the expectations of the
default configuration files.

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

TERA Arise is currently distributed in binary form without a license. If we
publish the project as open source in the future, we will be using the
[GNU Affero General Public License 3.0](LICENSE-AGPL-3.0).
