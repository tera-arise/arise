# TERA Arise

TERA Arise is a game resurrection project for the final build of TERA EU.

## Confidentiality

This is currently a private project. Until that changes, under no circumstances
should code, documentation, assets, etc be shared with anyone outside the
development team.

An obvious exception exists for binary artifacts that need to be distributed to
clients. Broadly speaking, this includes most things in
[`src/shared`](src/shared) and [`src/client`](src/client).

## Building

Building and running TERA Arise requires the following:

* .NET SDK 7.0.2+
* GnuTLS 3.7.8+
* PostgreSQL 14.6+

Some .NET global tools are used during the build process, so you will need to
run `dotnet tool restore` before building. After that, simply run `dotnet build`
to build client and server artifacts.

By default, `dotnet build` will build in the `Debug` configuration, which is
suitable for local development. The `Release` configuration (i.e.
`dotnet build -c Release`) is used for staging and production deployments.

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
