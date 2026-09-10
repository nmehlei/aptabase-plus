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
- A new, additive `/api/v0/apps` and `/api/v0/api-keys` surface, kept
  separate from the internal `/api/_apps` routes so the SPA team can keep
  reshaping those freely without breaking automation that depends on a
  stable contract.

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
