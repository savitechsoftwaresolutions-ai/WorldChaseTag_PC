# UX / UI Design System

> This document defines reusable visual and interaction rules. It must follow the approved `PRD.md`.

## 1. Design principles
- Make the primary task obvious within the first view.
- Prefer clarity and task completion over decoration.
- Use consistent patterns for equivalent actions.
- Preserve user input during recoverable errors.
- Provide visible feedback for actions and background work.
- Avoid dark patterns, fake urgency, and misleading success states.

## 2. Design direction
Document product-specific visual tone, density, and brand constraints. Do not default to a generic gradient-heavy SaaS layout. Every visual choice should support readability, hierarchy, or task completion.

## 3. Tokens
Define semantic tokens for:
- Background / surface / elevated surface
- Text primary / secondary / disabled
- Border / divider
- Primary action / secondary action
- Success / warning / danger / information
- Focus ring
- Spacing scale
- Radius scale
- Elevation
- Motion duration and easing

Use semantic names rather than hard-coded color values inside components. Record exact values only after brand approval.

## 4. Typography
Define font families, fallback fonts, type scale, line heights, weights, and maximum readable line lengths. Use tabular numerals for metrics where helpful. Ensure language and glyph coverage.

## 5. Layout and responsive rules
Specify breakpoints based on content needs, page max-widths, grid behavior, safe areas, mobile navigation, touch targets, and overflow rules. Mobile layouts must not simply shrink desktop layouts.

## 6. Component standards
For each shared component define purpose, variants, sizes, states, keyboard behavior, responsive behavior, and do/don't guidance:
- Buttons and icon buttons
- Inputs, selects, text areas, date/time controls
- Form field, helper text, validation message
- Dialog, drawer, popover, tooltip
- Table, list, pagination, filter bar
- Tabs, breadcrumbs, navigation
- Cards and summary metrics
- Toast / inline alert
- Skeleton / progress indicator
- Empty state
- Error state
- File upload and media preview
- Charts and status badges

## 7. Interaction rules
- State change must be communicated.
- Destructive actions require clear consequences and confirmation when material.
- Use optimistic updates only where rollback is safe and visible.
- Prevent accidental double submission.
- Respect reduced-motion settings.
- Avoid animation that delays task completion.

## 8. Forms and validation
Use labels that remain visible, field-level validation, clear error messages, correct input types, and server-side validation. Explain constraints before submission when useful. Preserve valid entered data on failure.

## 9. Data-heavy screens
Define sorting, filtering, pagination, column visibility, bulk actions, selection behavior, stale data indicators, and export states. Do not overload mobile screens with desktop-width tables.

## 10. Loading, empty, and error states
For each screen specify:
- Initial loading and refresh behavior
- Empty state with useful next action
- Partial-data state
- Recoverable and unrecoverable errors
- Permission denied and expired session
- Offline / retry behavior where relevant

## 11. Accessibility
Target WCAG 2.2 AA where applicable:
- Keyboard access and visible focus
- Semantic structure and accessible names
- Contrast and non-color status indicators
- Screen-reader announcements for async changes
- Accessible error association
- Touch targets appropriate to platform
- Reduced motion
- Zoom and text scaling
- Captions/transcripts for relevant media

## 12. Content and localization
Use concise, plain-language labels. Define date, time, number, currency, pluralization, timezone, and translation behavior. Do not concatenate translated sentence fragments.

## 13. Page-level specifications
For each page describe hierarchy, primary action, secondary actions, data displayed, component composition, responsive changes, and all key states.

## 14. Design QA checklist
- [ ] Matches PRD user journey and permissions
- [ ] Shared components reused
- [ ] Mobile and desktop behavior verified
- [ ] Keyboard and screen reader basics verified
- [ ] Loading, empty, and error states exist
- [ ] Validation messages are understandable
- [ ] No placeholder content or fake data in production
