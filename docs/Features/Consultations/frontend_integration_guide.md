# Paid Consultations Frontend Integration Guide

> **Feature:** paid lawyer consultations (`استشارة`)  
> **Audience:** SmartCourt frontend and QA teams  
> **Source of truth:** `feature/lawyer-consultations` backend implementation  
> **Scope:** consultation discovery, lawyer setup, availability, booking, payment, completion, cancellation, disputes, and settlement

## 1. Business Flow

Paid consultations are independent from cases, proposals, contracts, and a lawyer's existing case-availability flag. A lawyer can be unavailable for case work and still offer consultations.

1. A lawyer enables consultation services.
2. The lawyer creates one or more offerings: phone, video, or in-office.
3. Each offering has its own specialization, duration, price, inclusions, active status, and available slots.
4. A client discovers a lawyer, selects an offering and slot, and creates a booking.
5. The slot is reserved for 10 minutes while the client pays.
6. The server loads and snapshots the offering price. The frontend never calculates or submits a payable amount.
7. Successful payment confirms the booking and creates an escrow hold.
8. The lawyer performs the consultation and marks it performed.
9. The client confirms completion or opens a dispute. No client response for 24 hours completes it automatically.
10. Undisputed earnings remain on hold for 14 days, then the lawyer's net amount becomes available in the existing wallet.

The client pays the displayed gross price. SmartCourt keeps 5%; the lawyer receives 95%.

| Client price | Platform fee | Lawyer net |
|---:|---:|---:|
| 250 EGP | 12.50 EGP | 237.50 EGP |
| 1,000 EGP | 50 EGP | 950 EGP |

## 2. Important Frontend Rules

- Send `Authorization: Bearer {jwt}` on protected endpoints.
- Store all API dates as UTC ISO-8601 values and convert them to the user's local timezone only for display.
- Use each booking's `permittedActions` array to show buttons. Do not recreate the state machine in React.
- Do not send price, fee, lawyer ID, duration, mode, location, or inclusions when booking. The backend snapshots them from the selected offering.
- The exact office address and video meeting URL are private. They are `null` until payment succeeds and are returned only to the booking participants or administrators.
- For active paid phone consultations, `clientPhoneNumber` is returned only to the assigned lawyer or an administrator, and only when the client's account phone is confirmed. It remains `null` for the client, public responses, unpaid bookings, non-phone modes, and closed/disputed bookings.
- A consultation setting, offering, and slot must all be active/bookable before discovery and booking succeed.
- Default page size is 5; maximum page size is 50.
- Query-array filters may be repeated, for example `?modes=1&modes=3&specializations=4&specializations=7`.
- Keep one `Idempotency-Key` UUID for the same payment attempt and network retries. Generate a new key only for a genuinely new attempt.

## 3. Enums

ASP.NET may serialize enums as strings in the current API responses; query parameters and request bodies also accept the numeric values shown here. Frontend TypeScript should model the names and tolerate numeric values during integration.

### Consultation mode

| Value | Name | UI label |
|---:|---|---|
| 1 | `Phone` | Phone call |
| 2 | `VideoMeeting` | Video meeting |
| 3 | `InOffice` | In-office meeting |

### Booking status

| Value | Name | Meaning |
|---:|---|---|
| 0 | `AwaitingPayment` | Slot reserved; payment must finish within 10 minutes. |
| 1 | `Confirmed` | Paid and scheduled. |
| 2 | `AwaitingClientConfirmation` | Lawyer marked the consultation performed. |
| 3 | `Completed` | Client confirmed or the 24-hour timer elapsed. |
| 4 | `Cancelled` | Cancelled without a completed payment. |
| 5 | `Expired` | Payment reservation expired. |
| 6 | `Disputed` | Funds are frozen for administrator settlement. |
| 7 | `Refunded` | Provider refund completed. |

### Slot status

| Value | Name |
|---:|---|
| 0 | `Available` |
| 1 | `Reserved` |
| 2 | `Booked` |
| 3 | `Blocked` |
| 4 | `Cancelled` |

### `permittedActions`

Possible values are `Pay`, `Cancel`, `MarkPerformed`, `ConfirmCompletion`, `OpenDispute`, and `SettleDispute`. Render an action only when its name is present.

## 4. Endpoint Catalog

### Public discovery

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/consultations/lawyers` | Search enabled consultation lawyers. |
| `GET` | `/api/consultations/lawyers/{lawyerId}` | Get one lawyer and active offerings. |
| `GET` | `/api/consultations/offerings/{offeringId}/slots` | Get public available slots. |

### Lawyer management

All routes require the `Lawyer` role.

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/consultations/lawyer/settings` | Read consultation opt-in and scheduling rules. |
| `PUT` | `/api/consultations/lawyer/settings` | Enable/disable consultations and update scheduling rules. |
| `GET` | `/api/consultations/lawyer/offerings` | Read all owned offerings, including inactive ones. |
| `POST` | `/api/consultations/lawyer/offerings` | Create an offering. |
| `PUT` | `/api/consultations/lawyer/offerings/{offeringId}` | Update an offering. |
| `PATCH` | `/api/consultations/lawyer/offerings/{offeringId}/status` | Activate/deactivate an offering. |
| `POST` | `/api/consultations/lawyer/offerings/{offeringId}/slots` | Create up to 100 UTC slots. |
| `GET` | `/api/consultations/lawyer/offerings/{offeringId}/slots` | Read owned slots, including unavailable states. |
| `DELETE` | `/api/consultations/lawyer/slots/{slotId}` | Cancel a free future slot. |
| `GET` | `/api/consultations/lawyer/bookings` | List the lawyer's bookings. |

### Client booking

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/consultations/bookings` | Reserve a slot and create an unpaid booking. |
| `POST` | `/api/consultations/bookings/{bookingId}/payment-session` | Pay using a Stripe ConfirmationToken. |
| `GET` | `/api/consultations/client/bookings` | List the logged-in client's bookings. |
| `GET` | `/api/consultations/bookings/{bookingId}` | Read an authorized booking. |
| `POST` | `/api/consultations/bookings/{bookingId}/cancel` | Cancel according to the refund policy. |
| `POST` | `/api/consultations/bookings/{bookingId}/confirm-completion` | Confirm the performed consultation. |
| `POST` | `/api/consultations/bookings/{bookingId}/disputes` | Freeze funds and open a dispute. |

### Lawyer delivery

| Method | Route | Purpose |
|---|---|---|
| `PUT` | `/api/consultations/bookings/{bookingId}/delivery-details` | Set/update an HTTPS URL for a confirmed video booking. |
| `POST` | `/api/consultations/bookings/{bookingId}/mark-performed` | Mark a past confirmed consultation performed. |

### Finance administration

Requires `FinanceAdministrator` or `SuperAdministrator`.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/admin/consultations/bookings/{bookingId}/settle-dispute` | Split a disputed gross payment between client refund and lawyer settlement. |

## 5. Lawyer Setup

### Update settings

`PUT /api/consultations/lawyer/settings`

```json
{
  "isEnabled": true,
  "minimumBookingNoticeHours": 2,
  "maximumAdvanceBookingDays": 60,
  "bufferMinutes": 15,
  "timeZoneId": "Africa/Cairo"
}
```

Validation ranges:

| Field | Rule |
|---|---|
| `minimumBookingNoticeHours` | 0 to 168 |
| `maximumAdvanceBookingDays` | 1 to 365 |
| `bufferMinutes` | 0 to 120 |
| `timeZoneId` | Valid system timezone ID; use `Africa/Cairo`. |

Disabling settings removes the lawyer from public discovery but preserves existing data and bookings.

### Create offering

`POST /api/consultations/lawyer/offerings`

```json
{
  "mode": "Phone",
  "specialization": "RealEstateAndPropertyRegistration",
  "title": "Property purchase consultation",
  "description": "Review ownership concerns, contract clauses, and property registration risks.",
  "durationMinutes": 45,
  "price": 250.00,
  "officeLocation": null,
  "inclusions": [
    "45-minute consultation",
    "Initial document review",
    "Written next-step summary"
  ],
  "isActive": true
}
```

Rules:

- Duration: 15 to 240 minutes.
- Price: greater than 0, maximum 100,000 EGP, at most two decimal places.
- Inclusions: 1 to 10 unique items, each up to 200 characters.
- The specialization must already belong to the lawyer's profile.
- `officeLocation` is required only for `InOffice` and rejected for other modes.
- In Stripe Connect mode, activating an offering requires a payout-ready lawyer account.

Recommended UI: create separate offering cards or rows for each mode/rate, with a clear active toggle. Do not put consultation fields into the existing lawyer-profile edit form.

### Create slots

`POST /api/consultations/lawyer/offerings/{offeringId}/slots`

```json
{
  "slots": [
    { "startAtUtc": "2026-08-20T16:00:00Z" },
    { "startAtUtc": "2026-08-20T17:00:00Z" }
  ]
}
```

The backend derives `endAtUtc` from the offering duration and rejects duplicates, overlaps, buffer conflicts, insufficient notice, and slots beyond the advance-booking horizon.

## 6. Discovery and Filters

Example:

```http
GET /api/consultations/lawyers?modes=1&modes=2&specializations=4&minimumPrice=200&maximumPrice=600&availableFromUtc=2026-08-20T00:00:00Z&availableToUtc=2026-08-27T23:59:59Z&search=property&page=1&pageSize=5
```

Response page shape:

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "lawyerId": "GUID",
        "name": "Mona Adel",
        "profilePictureUrl": null,
        "governorate": "Cairo",
        "city": "Nasr City",
        "averageRating": 4.8,
        "isAcceptingConsultations": true,
        "isBookable": true,
        "unavailableReason": null,
        "startingPrice": 250.00,
        "currency": "EGP",
        "nextAvailableAtUtc": "2026-08-20T16:00:00Z",
        "offerings": []
      }
    ],
    "page": 1,
    "pageSize": 5,
    "totalRecords": 1,
    "totalPages": 1
  },
  "statusCode": 200
}
```

Use `isBookable` for the main CTA. When false, disable booking and show `unavailableReason`.

## 7. Client Booking and Payment

### Create booking

`POST /api/consultations/bookings`

```json
{
  "offeringId": "OFFERING_GUID",
  "slotId": "SLOT_GUID",
  "subject": "Apartment purchase review",
  "matterSummary": "I need the ownership chain and preliminary sale contract reviewed before paying the deposit."
}
```

The response is `AwaitingPayment` and includes `paymentExpiresAtUtc`. Start a visible countdown using that server value. If it expires, return the user to slot selection.

### Pay

Use the same Stripe ConfirmationToken browser pattern documented in `docs/Payments_API_Integration_Guide.md`.

```http
POST /api/consultations/bookings/{bookingId}/payment-session
Authorization: Bearer {clientJwt}
Idempotency-Key: 3f3e3504-6b49-45b8-8763-4db51e536bd1
Content-Type: application/json
```

```json
{
  "confirmationTokenReference": "ctoken_..."
}
```

The backend amount is authoritative. Handle `clientActionType`, `clientSecret`, and `redirectUrl` when returned. Provider webhooks are authoritative for asynchronous success/failure; refresh `GET /api/consultations/bookings/{bookingId}` after provider action.

After successful funding:

- status becomes `Confirmed`;
- the slot becomes `Booked`;
- the participant can see the in-office address or meeting URL;
- the lawyer's 95% net is tracked as pending, not withdrawable.

## 8. Booking Response

```json
{
  "id": "BOOKING_GUID",
  "offeringId": "OFFERING_GUID",
  "slotId": "SLOT_GUID",
  "lawyerId": "LAWYER_GUID",
  "lawyerName": "Mona Adel",
  "clientId": "CLIENT_GUID",
  "clientName": "Omar Hassan",
  "clientPhoneNumber": null,
  "mode": "InOffice",
  "specialization": "RealEstateAndPropertyRegistration",
  "offeringTitle": "Property contract review",
  "inclusions": ["45-minute consultation", "Initial document review"],
  "durationMinutes": 45,
  "grossAmount": 250.00,
  "platformFeeAmount": 12.50,
  "lawyerNetAmount": 237.50,
  "currency": "EGP",
  "subject": "Apartment purchase review",
  "matterSummary": "I need the ownership chain reviewed before paying the deposit.",
  "startAtUtc": "2026-08-20T16:00:00Z",
  "endAtUtc": "2026-08-20T16:45:00Z",
  "status": "Confirmed",
  "paymentExpiresAtUtc": "2026-08-15T13:10:00Z",
  "officeLocation": "Nasr City office, Cairo",
  "meetingUrl": null,
  "performedAtUtc": null,
  "completedAtUtc": null,
  "cancellationReason": null,
  "disputeReason": null,
  "payment": {},
  "permittedActions": ["Cancel"]
}
```

Before payment, `officeLocation` and `meetingUrl` are `null` even if the offering contains them.

For a paid `Phone` booking, the assigned lawyer receives the client's confirmed number:

```json
{
  "mode": "Phone",
  "status": "Confirmed",
  "clientPhoneNumber": "+201001234567"
}
```

The client receives `clientPhoneNumber: null` for the same booking. The lawyer can receive it only while status is `Confirmed` or `AwaitingClientConfirmation`; it disappears after completion, cancellation, expiry, refund, or dispute. The frontend must show the number only in the lawyer's active phone-appointment view and must not copy it into notifications, analytics, logs, URLs, or shared browser storage. When it is `null` on an active phone booking, tell the lawyer that the client's confirmed contact number is unavailable and route the issue to support; do not fall back to a public profile endpoint.

## 9. Completion, Cancellation, and Disputes

### Video delivery details

For a paid video consultation, the lawyer may set a private HTTPS link:

```json
PUT /api/consultations/bookings/{bookingId}/delivery-details
{
  "meetingUrl": "https://meet.example.com/mostashar-abc123"
}
```

### Mark performed

```json
POST /api/consultations/bookings/{bookingId}/mark-performed
{
  "meetingUrl": "https://meet.example.com/mostashar-abc123"
}
```

`meetingUrl` is optional here and useful for video consultations. The booking must be confirmed and its scheduled end time must have passed.

### Client decision

- `ConfirmCompletion` completes the booking and starts the 14-day release hold.
- `OpenDispute` freezes the funds and requires a reason of 20 to 2,000 characters.
- If the client does neither for 24 hours after performance, the backend auto-completes the booking.

### Cancellation policy

- Unpaid booking: cancellation frees the slot immediately.
- Lawyer cancellation: full client refund and slot becomes available again when appropriate.
- Client cancellation at least 24 hours before start: full refund.
- Client cancellation within 24 hours of start: funds are frozen as a dispute for administrator review.

Cancellation body:

```json
{
  "reason": "I no longer need this consultation."
}
```

### Administrator settlement

```json
POST /api/admin/consultations/bookings/{bookingId}/settle-dispute
{
  "clientRefundAmount": 100.00,
  "reason": "Partial refund after reviewing the consultation record."
}
```

`clientRefundAmount` is a gross amount between 0 and the booking gross price:

- gross amount: full refund;
- zero: full release to lawyer after platform fee;
- between them: partial client refund and proportional lawyer/platform settlement.

## 10. List Filters and Suggested Screens

Both booking lists support repeated statuses, UTC date range, and pagination:

```http
GET /api/consultations/client/bookings?statuses=0&statuses=1&statuses=2&page=1&pageSize=5
GET /api/consultations/lawyer/bookings?statuses=1&statuses=2&fromUtc=2026-08-01T00:00:00Z&page=1&pageSize=5
```

Suggested client views:

- Discover lawyers with mode, specialization, price, availability, and search filters.
- Lawyer details with offerings and inclusion lists.
- Slot picker in local time with timezone label.
- Checkout with server-price summary and reservation countdown.
- My consultations split into upcoming, action required, completed, and closed.

Suggested lawyer views:

- Consultation settings with one master enabled toggle.
- Offerings list with per-offering active toggle.
- Calendar/slot management by offering.
- Bookings with upcoming and action-required filters.
- Existing wallet and withdrawal screens; no separate consultation wallet is needed.

Suggested administrator view:

- Disputed consultations queue with payment breakdown, reason, participants, and settlement amount input.

## 11. Errors and Security

- `400 Bad Request`: validation failure or malformed request.
- `401 Unauthorized`: missing/invalid JWT.
- `403 Forbidden`: authenticated role is not allowed.
- `404 Not Found`: resource does not exist **or is not owned by/visible to the caller**. Treat this as a privacy boundary.
- `409 Conflict`: stale lifecycle action, already reserved slot, overlap, expired reservation, invalid cancellation timing, or an offering/settings prerequisite is not satisfied.

Do not reveal whether another user's booking exists. Do not cache private booking responses in shared browser storage. Never collect raw card number, expiry, CVC, bank details, provider secrets, or webhook secrets in the SmartCourt frontend.

## 12. Frontend Delivery Checklist

- [ ] Add consultation enums and DTO types.
- [ ] Build anonymous discovery and lawyer detail pages.
- [ ] Build lawyer settings, offerings, and UTC slot management.
- [ ] Build client booking form and 10-minute payment countdown.
- [ ] Reuse Stripe ConfirmationToken checkout and idempotency behavior.
- [ ] Build role-specific booking lists with multi-status filters.
- [ ] Render actions exclusively from `permittedActions`.
- [ ] Hide private delivery details when API returns `null`.
- [ ] Build lawyer video-link and mark-performed actions.
- [ ] Build client completion/dispute actions.
- [ ] Reuse existing lawyer wallet and withdrawal screens.
- [ ] Handle `400`, `401`, `403`, `404`, and `409` without exposing private data.
