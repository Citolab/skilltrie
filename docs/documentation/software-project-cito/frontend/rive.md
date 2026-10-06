
# Rive 

Rive is the editor the artist uses to create (interactive) 2D artwork. The Rive editor exports files as `.riv` files, which together with a [runtime for the web](https://rive.app/docs/runtimes/react/react)
can be used to integrate this art into the React frontend. There are multiple ways the runtime can do this, e.g. via setting up and rendering to a WebGL context, or by making use
of the `<canvas>` API. We choose the latter, as it's more lightweight, and we're only dealing with 2D animations. 

## Integration

Programmatically, a minimal integration in React looks as follows:

```tsx
import { useRive } from '@rive-app/react-canvas';

export default function Simple() {
    // integrating Rive components via the useRive()
    // hook is the most common and recommended way.
    const { rive, RiveComponent } = useRive({
        src: '/path/to/riv/file.riv',
        stateMachines: "bumpy",
        autoplay: false,
    });

    return (
        <RiveComponent
            onMouseEnter={() => rive && rive.play()}
            onMouseLeave={() => rive && rive.pause()}
        />
    );
}
```

Rive's own [documentation](https://rive.app/docs/runtimes/react/react) goes into more detail on how to use to runtime.

::: info
Many of the terminology of Rive's react runtime library has a direct correspondence with
concepts that are inherent to Rive's editor program. It may be helpful to sit down with the artist and their editor to get
a clearer view of the semantics of some of this terminology.
\
There's also documentation on this, found [here](https://rive.app/docs/runtimes/getting-started), under *Runtime Fundamentals*.
:::