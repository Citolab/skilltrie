# A/B tests

When doing AB tests in the backend of our project, an interface to the Feature Flags is exposed through the `AbTestingService` in the backend. This interface currently is currently implemented for Posthog, but it should be easy to change to other Feature Flag managers as well. Below is a breakdown of how A/B tests are designed in our backend. 

## The `AbTest` static class

For starters, our Handlers folder contains a file called **AbTestingHandler**, that contains some structs and most importantly the AbTest class. This class contains one (1) overloaded method called `Run()`. As its arguments, it accepts the feature flag, and a function that should use this feature flag. The feature flag can be requested using the **AbTestingService**, which holds a class with one (1) async method called `GetFlagAsync()`. 

The general form looks as follows: 

```C#
AbTest.Run(
    await abService.GetFlagAsync(userId, "urningsAlgorithm"),
    flag => {
        doSomethingAwesome(flag);
    }
)
```

And with that, we have created an AbTest that resolves the flag in the `doSomethingAwesome()` method. From here on out, I will go into more detail on how to use this interface. 

## `GetFlagAsync()`

This method is meant to be called whenever you want to acquire a flag based on a userId and key. It also deserialises the payload and makes it dynamic, which is an added bonus so that it becomes easier to work with. The return type is `FlagResult`, that contains a bool `IsEnabled`, a string `Variant` and a `SafePayload` `Payload`. The safe payload is meant to simply return null when you try to access a payload that doesn't exist. This is because researchers may change payloads at any time, and we don't want this to break our app. 

## AbTest.Run() as an if-statement

If you want to run AbTest.Run() as a simple boolean, you can. This is because the code inside of the `Action` of `AbTest.Run()` simply won't execute in case either the `isEnabled` flag is false, or the flag is null (does not exist). So, you can safely call methods inside of the `AbTest.Run()` block without having to worry about code breaking when you disable the flag or change a name of something. 

## Returning values with AbTest.Run()

It is also possible to return values, as the method is overloaded. It is possible to do things like

```C#
int payloadValue = AbTest.Run(
    await abService.GetFlagAsync(userId, "someValue"),
    flag => {
        return flag.Payload.Value; // Value will differ depending on user
    }
);
```

## AbTest.Run() defaults

To go even further, it is also possible to define a default behaviour using the overwrite method. This way, if a key was not found or a flag was disabled, you can still define some default behaviour. An example of this is: 

```C#
var value = AbTest.Run(
    await abService.GetFlagAsync(userId, "someValue"),
    flag => {
        return flag.Payload.Value; // Value will differ depending on user
    }
) ?? someDefaultValue;
```

Or something like

```C#
var value = AbTest.Run(
    await abService.GetFlagAsync(userId, "someValue"),
    flag => {
        return flag.Payload.Value; // Value will differ depending on user
    }
); 

if (value is null)
{
    doSomethingDefault(); 
}
```

## Multivariates and pattern matching

You can also do cool stuff with multivariate feature flags, like

```C#
AbTest.Run(
    await abService.GetFlagAsync(userId, "algorithm"),
    flag => flag.Variant switch
    {
        "control"   => MethodA(),
        "treatment" => MethodN(),
        _           => oldMethod()
    }
);
```

## Ignoring the flag

It is easy to ignore the flag. Just use a hole in the place where the flag would be in the lambda:


```C#
AbTest.Run(
    await abService.GetFlagAsync(userId, "someNewFeature"),
    _ => {
        features.Add("someNewFeature");
    }
);
```

## Guarding features

You can easily guard features to only be visible for certain groups, by setting

```C#
AbTest.Run(
    await abService.GetFlagAsync(userId, "someNewFeature"),
    () => {
        features.Add("someNewFeature");
    }
);
```

## Async

It is also possible to make the entire method async. This should be done whenever the Action is also async, as otherwise the app might fail silently as the awaited methods may never finish and the compiler won't be able to figure this out (sad face). 

```C#
await AbTest.RunAsync(
    await abService.GetFlagAsync(userId, "urningsAlgorithm"),
    async flag => {
        await doSomethingAwesomeButAsynchronously(flag);
    }
)
```