# Posthog
We use posthog to acquire web analytics and perform feature flagging and A/B testing. 

## Current captured events: 
(Auto-generated):

| Event | Description | File |
|---|---|---|
| `user_logged_in` | User successfully logged in | `src/pages/login.tsx` |
| `user_login_failed` | Login attempt failed | `src/pages/login.tsx` |
| `user_registered` | New account created | `src/pages/register.tsx` |
| `user_registration_failed` | Registration attempt failed | `src/pages/register.tsx` |
| `level_started` | User clicked "Start Level" on a topic node | `src/components/tree/node-tooltip/node-tooltip.tsx` |
| `level_completed` | User finished all questions and submitted | `src/components/qti-player/main-page/qti-player.tsx` |
| `level_quit` | User quit an in-progress level | `src/components/qti-player/main-page/qti-player.tsx` |
| `question_reported` | User submitted a report on a question | `src/components/qti-player/popups/report-form.tsx` |