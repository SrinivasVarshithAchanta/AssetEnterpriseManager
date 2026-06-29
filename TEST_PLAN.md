# Manual Test Plan

Manual test cases to verify the app end to end. Run after `dotnet run` with the seeded demo accounts.

| # | Area | Steps | Expected result |
|---|------|-------|-----------------|
| 1 | Login (valid) | Sign in as `admin@assetops.com / Admin@123` | Redirected to Dashboard; name + Admin badge in top bar |
| 2 | Login (invalid) | Sign in with a wrong password | Stay on login; "Invalid email or password" message |
| 3 | Role redirect | Sign in as manager, then as employee | Manager lands on Approvals; Employee lands on My Requests |
| 4 | Employee request creation | As employee, create a request (pick category, priority, reason ≥10 chars) | Request appears in My Requests with status Pending and a REQ number |
| 5 | Validation | As employee, submit a request with a 3-character reason | Form blocked with "at least 10 characters" message |
| 6 | Live availability | On the create-request page, change the category | Helper text shows "N asset(s) currently available" (calls the API) |
| 7 | Cancel request | As employee, cancel a pending request | Status becomes Cancelled; confirm dialog shown first |
| 8 | Manager approval | As manager, open a pending request and Approve | Status becomes Approved; reviewer recorded |
| 9 | Manager rejection (no comment) | As manager, Reject without a comment | Blocked with "A comment is required" message |
| 10 | Manager rejection (with comment) | As manager, Reject with a comment | Status becomes Rejected; comment stored and shown |
| 11 | Admin asset creation | As admin, add an asset (unique tag) | Asset appears in the list; success message |
| 12 | Duplicate tag | As admin, add an asset with an existing tag | Blocked with "asset tag is already in use" |
| 13 | Future purchase date | As admin, set a purchase date in the future | Blocked with "cannot be in the future" |
| 14 | Admin fulfilment | As admin, open an Approved request, Fulfill, pick an available asset | Request becomes Fulfilled; asset becomes Assigned to the requester |
| 15 | Retire asset | As admin, retire an asset | Status becomes Retired; cannot be assigned afterwards |
| 16 | Category management | As admin, add a category, then deactivate it | New category appears; deactivated category no longer offered on request form |
| 17 | User management | As admin, create a user, then deactivate them | User appears; deactivated user cannot log in (case 19) |
| 18 | Unauthorized access | As employee, browse to `/Users` or `/Approvals` | Access Denied (403) page |
| 19 | Inactive login | Deactivate a user, then try to log in as them | Login refused |
| 20 | Pagination & filters | On Users list, search a name and page through results | Filter persists across pages; counts correct |
| 21 | Audit log | As admin, open Audit Logs after some actions | Entries for create/approve/reject/fulfill are present |
| 22 | Logout | Click Logout | Returned to login page; protected pages redirect to login |
