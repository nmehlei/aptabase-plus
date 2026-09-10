# Upstream proposal: API keys for account-management endpoints

Tracks [aptabase/aptabase#145](https://github.com/aptabase/aptabase/issues/145).

## Problem

Aptabase's account-management endpoints (`/api/_apps` and friends) are
authenticated only via an ASP.NET Core cookie session, set through
GitHub/Google OAuth or a magic-link email flow. There is no way for
non-interactive tooling (CI, Terraform, scripts) to authenticate.

## Proposal

- One key type: user-scoped, inheriting the full permissions of its
  owning user (including minting/revoking further keys for that user).
- Format `aptb_<32 random bytes, base64url>`, stored as a SHA-256 hash
  plus a 12-character display prefix; plaintext shown once at creation.
- New `api_keys` table (`id`, `user_id`, `name`, `key_hash`, `key_prefix`,
  `last_used_at`, `expires_at`, timestamps).
- New ASP.NET Core authentication scheme selected via a policy scheme
  based on the presence of `Authorization: Bearer` — every existing
  `[IsAuthenticated]` endpoint accepts a key with zero changes.
- An **extension of the existing, upstream-owned `/api/v0/` prefix** with
  `/api/v0/apps` and `/api/v0/api-keys` routes, kept separate from the
  internal `/api/_apps` routes so the SPA team can keep reshaping those
  freely without breaking automation that depends on a stable contract.
  This is **not** a new, additive namespace: upstream already serves
  `/api/v0/event`, `/api/v0/events`, `/api/v0/feature-flags/*`, and
  `/api/v0/apps/{appId}/errors`, `/errors/types`, `/errors/{errorId}`
  (`ErrorsController.cs`) under the same prefix.

### Exact paths this fork claims under `/api/v0/`

- `GET`, `POST` `/api/v0/apps`
- `GET`, `PUT`, `DELETE` `/api/v0/apps/{appId}`
- `GET` `/api/v0/apps/{appId}/shares`
- `PUT`, `DELETE` `/api/v0/apps/{appId}/shares/{email}`
- `GET`, `POST` `/api/v0/api-keys`
- `DELETE` `/api/v0/api-keys/{keyId}`

**Known risk:** since `/api/v0/` is shared with upstream, a future
upstream change under that prefix could collide with these routes when
this fork rebases on `aptabase/main`. Upstreaming this surface (or
agreeing a reserved sub-prefix) would remove the risk.

## Status

Implemented and running in
[aptabase-plus](https://github.com/nmehlei/aptabase-plus), a downstream
distribution. Not yet posted as a comment on #145 — pending owner approval. A scoped-down PR is
available on request.

## Relation to the original #145 proposal

The original issue proposed per-app `base64(AppId:ClientSecret)` basic
auth aimed at read-only analytics automation. This proposal uses
account-scoped bearer tokens instead, aimed at write access for
infrastructure-as-code tooling (Terraform). Both could coexist as
separate, differently-scoped credential types if there's interest.
