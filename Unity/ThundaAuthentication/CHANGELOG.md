# changelog

## [1.0.0]

first release.

- `LoginWithOculus`, does the whole nonce + preauth + attest + login flow in one call
- sets the playfab session ticket after login so the playfab sdk works straight away
- `CallApi<T>` for your own jwt routes
- `ThundaEvents` for login / logout hooks
- `ThundaDebug` toggle for logs
- editor window under Thunda > Settings
- login example sample
