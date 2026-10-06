# AB Testing

AB testing has been implemented in a way where it separates into three concerns. It is simple and modular, where basically all the work for setting up an AB test is creating the new components and deciding on the parameters for the feature flags.

## Model
The Model for an AB-test is in "hooks/ab-testing.ts". It contains the type ABTest, which contains the key (the name of the feature flag), and a mapping of all possible variants to their concrete component. This component is a general Type and can be anything; a Button, a string, etc.

This file also features a function useABTest, which will, given an `ABTest<T>`, reads the feature flag value from the PostHog SDK, which keeps flag assignments synchronized with PostHog in the background, and return a T based on the flag associated with the user.

## AB Tests
For each feature which you want to AB-test, there should be a file in "utils/ab-tests/{feature}.tsx". This file exports an `ABTest<T>`. Thus, it defines the mapping from flag to variants, the default, and the key it is used for.

If you make an AB-test, make sure to also add it as a Feature flag into Posthog. The variant strings are literally taken from there, so make sure they match. The feature flag may also decide none of the variants are applicable, in this case it will take the 'default' component.

## Calling AB tests
Everywhere you want to have an AB-testable feature, you have to import useABTest and the specific `ABTest<T>` for your feature. You can call the function and pass it said `ABTest<T>`, and it will return you the correct component. If there are any common attributes for this component (for example: a button which always has text "Click me!", but has varying colors), then inside the function calling the Hook, you can make this mutation to the component.

## Example code
Heres the code I used to debug the functionality, which will of course be gone

### "utils/ab-tests/debug-ab-test.tsx"

``` tsx
import GeneralButton from "../../components/general-components/general-button.tsx";
import RequiredAsterix from "../../components/general-components/required-asterix.tsx";
import type { ABTest }  from "../../hooks/ab-testing.ts"; 
import type { ComponentType } from "react"; 

export const debugAbTest: ABTest<ComponentType<any>> = {
    flag: 'debug',
    variants: {
        'control': (props) => <GeneralButton {...props} variant="primary"/>,
        'test-1':  (props) => <GeneralButton {...props} variant="secondary"/>,
        'test_2':  (props) => <GeneralButton {...props} variant="ghost"/>
    },
    default: RequiredAsterix,
}
```

### Landing page (or wherever you want it implemented)
```tsx
import { useABTest } from "../hooks/ab-testing";
import { debugAbTest } from "../utils/ab-tests/debug-ab-test";

const AbButton = () => { 
    const Button = useABTest(debugAbTest);
    return <Button>AB</Button>
};

// In this case, the text AB is the common denominator between all variants.
```