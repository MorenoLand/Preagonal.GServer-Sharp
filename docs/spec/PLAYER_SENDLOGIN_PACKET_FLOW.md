# Player::sendLogin Packet Flow

## Normal Client Success Boundary

```txt
SVI_VERIACC2 SUCCESS
  -> Player::sendLogin()
  -> loadAccount(account, false)
  -> banned/staff/admin-ip checks
  -> send PLO_SIGNATURE, GCHAR 73
  -> optional login-server-name branch (blocked: PLO_FULLSTOP missing)
  -> send PLO_UNKNOWN168 for clients
  -> duplicate account/session check
  -> STOP before Server::playerLoggedIn
```

Confirmed queued bytes for the normal non-login-server client path:

```txt
[57, 105, 10, 200, 10]
```

## Rejection Before Early Packets

These cases send only `PLO_DISCMESSAGE + message + "\n"`:

- banned account without `PLPERM_MODIFYSTAFFACCOUNT`
- RC/NC without staff rights or admin IP
- client on staff-only server without staff rights
- client admin IP mismatch without `0.0.0.0` in `IPRANGE`

## Duplicate Active Client Rejection

C++ queues early client packets before checking duplicate accounts.

Confirmed order for active duplicate client rejection:

```txt
PLO_SIGNATURE + "\n"
PLO_UNKNOWN168 + "\n"
PLO_DISCMESSAGE "Account is already in use." + "\n"
```

## Stale Duplicate Client

If the matching duplicate's last data age is greater than 30 seconds:

```txt
existing session <- PLO_DISCMESSAGE "Someone else has logged into your account."
existing session disconnect()
current session continues to world-entry boundary
```

The current C# result records this as `DuplicateSessionDisconnect`; it does not disconnect another real network session yet.
