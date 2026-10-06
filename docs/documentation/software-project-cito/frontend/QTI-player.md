# QTI Player

The [QTI player](https://github.com/Citolab/qti-components) is not much more than a set of front-end
components made by CITO. It renders these components based on a QTI 3.0 `.xml` file. QTI is an XML-standard for exchanging and storing test-content. In QTI version 3.0 the
individual XML elements conveniently share the same syntax typical to HTML [Web Components](https://developer.mozilla.org/en-US/docs/Web/API/Web_components)
(more specifically, [custom elements](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_custom_elements)).
\
An example of an excerpt of this syntax:

```xml{1,5,6,8,9,11,13,14}
<qti-item-body>
    <p>
        Aan welke van de volgende voorwaarden moet zijn voldaan voor een enkelvoudige regressie-analyse waarin je de score op Y voorspelt uit de score op X?
    </p>
    <qti-choice-interaction response-identifier="qti_21_items_all_238788197_section_0002_item_1_schoice_RESPONSE_1_2485801" shuffle="false" max-choices="1" min-choices="1">
        <qti-simple-choice identifier="qti_21_items_all_238788197_section_0002_item_1_schoice_1_6116127029">
            <p>Het verband tussen X en Y moet lineair zijn.</p>
        </qti-simple-choice>
        <qti-simple-choice identifier="qti_21_items_all_238788197_section_0002_item_1_schoice_2_2759014746">
            ...
        </qti-simple-choice>
        ...
    </qti-choice-interaction>
</qti-item-body>
```

::: info
The QTI specification allows a subset of HTML tags to allow rudimentary styling of the question content (e.g. lists, images, paragraphs, etc.)
:::

## QTI Player vs. QTI XML File

As the name QTI _player_ implies, it's not much more than a frontend for the QTI standard (much like e.g. a video player). This has the implication that if you're seeking to modify behaviour of the QTI player, oftentimes you should be looking at modifying the contents of a QTI file instead.
\
For example:

- Want to change scoring logic? -> QTI file (`qti-response-processing`)
- Want to change feedback logic? -> QTI file
- Want to change the allowed no. of answers? -> QTI file
- Want to change styling? -> QTI player
- etc.

## QTI Standard

To better understand the components of the QTI player, it may be helpful to read up (just a little) on the QTI 3.0 standard. A [beginner's guide](https://www.imsglobal.org/spec/qti/v3p0/guide#introduction) (with illustrative pictures!) is made available by 1EdTech.

## Styling

You can _style_ the player's custom elements, but without using JavaScript to 'break into' the [shadow root](https://developer.mozilla.org/en-US/docs/Web/API/Web_components/Using_shadow_DOM) of the QTI player, your styling ability is **severly** limited. This is of course by design.

::: details
While it is strongly discouraged to mess around in the QTI player's shadow root, sometimes it may be unavoidable. For example, we had to manually resize images because they always rendered at their full resolution, leading to responsiveness issues on smaller screens.
:::

To style indivdual elements of the QTI player, you must set CSS variables on the `test-container` element:

```css
test-container {
    --qti-border-radius: 50%;
    --qti-bg: #fff;
    --qti-bg-active: #f1fdfa;
    --qti-border-active: #00968a;
    ...
}
```

**Which** variables you can set are defined [here](https://github.com/Citolab/qti-components/blob/main/packages/qti-theme/src/styles/qti-base.css) (look at the variables defined in `:root`). Note that different variables may affect different elements.

## Embedding the player (in React)

Since the QTI player is nothing more than just a few components, an absolute minimal integration will have the following structure:

```tsx
<qti-test>
    <test-navigation>
        <test-container testXML={assessmentXML}></test-container>
    </test-navigation>
</qti-test>
```

To get the TypeScript linter to pick up on these custom elements we have to augment React's `IntrinsicElements` interface. This requires a bit of boilerplate code that can be best put somewhere in the root of the frontend (e.g. `main.tsx`):

```tsx
import "@citolab/qti-components";
import type { CustomElements } from "@citolab/qti-components/react";

declare module "react" {
    namespace JSX {
        interface IntrinsicElements extends CustomElements {
            style: React.DetailedHTMLProps<
                React.StyleHTMLAttributes<HTMLStyleElement>,
                HTMLStyleElement
            >;
        }
    }
}
```

### Loading QTI files

When loading QTI files into the QTI player, you don't load the individual question files (although this is technically possible, but with another component),
but rather an **assessment** file. As explained in the standard, this is also a `.xml` file, but rather than containing an item, it groups multiple items together.
It does so not explicitly, but by listing where each item can be found using its URI:

::: code-group

```xml:line-numbers{14,16,18,20} [assessment.xml]
<?xml version="1.0" encoding="UTF-8"?>
<qti-assessment-test xmlns="http://www.imsglobal.org/xsd/imsqtiasi_v3p0"
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
xsi:schemaLocation="http://www.imsglobal.org/xsd/imsqtiasi_v3p0 
https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0p1_v1p0.xsd"
identifier="t1-test-entry"
title="T1 - test entry"
xml:lang="en-US">
    <qti-test-part identifier="testPart-1" navigation-mode="nonlinear" 
   submission-mode="individual">
        <qti-assessment-section identifier="assessmentSection-1" title="Section 1"
         visible="true">
            <qti-assessment-item-ref identifier="t1-test-entry-item1"
              href="items/choice-single-cardinality.xml"/>
            <qti-assessment-item-ref identifier="t1-test-entry-item2"
              href="items/choice-multiple-cardinality.xml"/>
            <qti-assessment-item-ref identifier="t1-test-entry-item3"
              href="items/text-entry.xml"/>
            <qti-assessment-item-ref identifier="t1-test-entry-item4"
             href="items/extended-text.xml"/>
        </qti-assessment-section>
    </qti-test-part>
</qti-assessment-test>
```

You feed the QTI player the assessment file, and the player will internally make a fetch request for each item upon navigating to that item.

:::

QTI assessment files are loaded into the player via a prop on the `<test-container>` tag:

```tsx
// via a string containing the raw XML
<test-container testXML={assessmentXML}></test-container>
```

```tsx
// via a URL
<test-container test-url="/link/to/the/assessment.xml"></test-container>
```

## Accessing test data

You can listen for when a user has interacted with the test (e.g. through answering or unanswering a question) by subscribing to the `"qti-outcome-changed"` event:

```tsx
document.addEventListener("qti-outcome-changed", (event) => {
    // use event.detail here for more information about what's changed.
});
```

`event.detail` in the example above won't hold much information about the test itself. If you want to access data like
which question is currently being displayed, and information about all the questions and the entire test as a whole, you'll need to subscribe to the `"qti-computed-context-updated"` event:

```tsx
document.addEventListener("qti-computed-context-updated", (event) => {
    // event.detail contains a lot of data about the test
});
```

Other QTI specific events you can subscribe to are:

::: warning DISCLAIMER
These were `grep`-ed from CITO's qti-components repository. There was no time available to research all of them to see what they represent and check whether they can all be subscribed to. There is little to no documentation about this from CITO themselves. You'll have to figure this out yourself.
:::

- `"qti-register-interaction"`
- `"qti-interaction-response"`
- `"qti-inline-choice-register"`
- `"qti-inline-choice-select"`
- `"qti-register-hotspot"`
- `"qti-portable-custom-interaction-loaded"`
- `"end-attempt"`
- `"qti-stamp-context-updated"`
- `"qti-assessment-item-ref-connected"`
- `"qti-computed-context-updated"`
- `"on-test-switch-view"`
- `"test-update-outcome-variable"`
- `"qti-request-navigation"`
- `"qti-assessment-test-connected"`
- `"test-show-correct-response"`
- `"qti-navigation-loading-started"`
- `"qti-navigation-loading-ended"`
- `"qti-navigation-error"`
- `"qti-test-loaded"`
- `"qti-test-context-updated"`
- `"qti-register-variable"`
- `"qti-template-processing-complete"`
- `"item-switch-correct-response-mode"`
- `"item-show-candidate-correction"`
- `"item-show-correct-response"`

## CITO Docs

CITO uses Storybook to provide (albeit very minimal) [interactive documentation](https://qti-components.citolab.nl/?path=/story/docs-hi-qti--default)

### Stackblitz examples

Examples of QTI integrations (without a frontend framework) can be found below:

- [Minimal Version](https://stackblitz.com/edit/citolab-bootcamp-starter-c7unqh2r?file=index.html)
- [Advanced Version](https://stackblitz.com/~/github.com/Citolab/qti-player?file=index.html)
