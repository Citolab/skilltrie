/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Dynamic;
using API.Handlers;

namespace APITests.Handlers;

public class AbTestingHandlerTest
{
    // SafePayload Tests 

    [Fact]
    public void SafePayload_ExistingProperty_ReturnsValue()
    {
        dynamic expando = new ExpandoObject();
        expando.proficiency = 0.1;

        dynamic payload = new SafePayload(expando);

        Assert.Equal(0.1, payload.proficiency);
    }

    [Fact]
    public void SafePayload_MissingProperty_ReturnsNullWithoutThrowing()
    {
        dynamic expando = new ExpandoObject();
        dynamic payload = new SafePayload(expando);

        var result = payload.nonExistentProperty;

        Assert.Null(result);
    }

    [Fact]
    public void SafePayload_NestedExpandoObject_IsWrappedInSafePayload()
    {
        dynamic inner = new ExpandoObject();
        inner.value = 67;

        dynamic outer = new ExpandoObject();
        outer.nested = inner;

        dynamic payload = new SafePayload(outer);

        Assert.IsType<SafePayload>(payload.nested);
    }

    [Fact]
    public void SafePayload_ChainedAccess_ReturnsNestedValue()
    {
        dynamic inner = new ExpandoObject();
        inner.value = 67;

        dynamic outer = new ExpandoObject();
        outer.nested = inner;

        dynamic payload = new SafePayload(outer);

        Assert.Equal(67, payload.nested.value);
    }

    [Fact]
    public void SafePayload_ChainedAccess_MissingNestedProperty_ReturnsNull()
    {
        dynamic inner = new ExpandoObject();

        dynamic outer = new ExpandoObject();
        outer.nested = inner;

        dynamic payload = new SafePayload(outer);

        Assert.Null(payload.nested.missingKey);
    }

    // AbTest.Run (void, no flag data) 

    [Fact]
    public void Run_Action_FlagEnabled_ExecutesAction()
    {
        var flag = new FlagResult(IsEnabled: true, Variant: null, Payload: null);
        var executed = false;

        AbTest.Run(flag, _ => executed = true);

        Assert.True(executed);
    }

    [Fact]
    public void Run_Action_FlagDisabled_DoesNotExecuteAction()
    {
        var flag = new FlagResult(IsEnabled: false, Variant: null, Payload: null);
        var executed = false;

        AbTest.Run(flag, _ => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void Run_Action_NullFlag_DoesNotExecuteAction()
    {
        var executed = false;

        AbTest.Run(null, _ => executed = true);

        Assert.False(executed);
    }

    // AbTest.Run<TResult> (returning, no flag data)

    [Fact]
    public void Run_Func_FlagEnabled_ReturnsResult()
    {
        var flag = new FlagResult(IsEnabled: true, Variant: null, Payload: null);

        var result = AbTest.Run(flag, _ => 67);

        Assert.Equal(67, result);
    }

    [Fact]
    public void Run_Func_FlagDisabled_ReturnsDefault()
    {
        var flag = new FlagResult(IsEnabled: false, Variant: null, Payload: null);

        var result = AbTest.Run(flag, _ => 67);

        Assert.Equal(default, result);
    }

    [Fact]
    public void Run_Func_NullFlag_ReturnsDefault()
    {
        var result = AbTest.Run<int>(null, _ => 67);

        Assert.Equal(default, result);
    }
    
    // AbTest.Run (void, with FlagResult) 

    [Fact]
    public void Run_ActionWithFlag_FlagEnabled_ExecutesWithFlagData()
    {
        var flag = new FlagResult(IsEnabled: true, Variant: "control", Payload: null);
        string? capturedVariant = null;

        AbTest.Run(flag, f => capturedVariant = f.Variant);

        Assert.Equal("control", capturedVariant);
    }

    [Fact]
    public void Run_ActionWithFlag_FlagDisabled_DoesNotExecute()
    {
        var flag = new FlagResult(IsEnabled: false, Variant: "control", Payload: null);
        string? capturedVariant = null;

        AbTest.Run(flag, f => capturedVariant = f.Variant);

        Assert.Null(capturedVariant);
    }

    [Fact]
    public void Run_ActionWithFlag_NullFlag_DoesNotExecute()
    {
        string? capturedVariant = "untouched";

        AbTest.Run(null, (FlagResult f) => capturedVariant = f.Variant);

        Assert.Equal("untouched", capturedVariant);
    }

    //  AbTest.Run<TResult> (returning, with FlagResult) 

    [Fact]
    public void Run_FuncWithFlag_FlagEnabled_ReturnsResultUsingFlag()
    {
        var flag = new FlagResult(IsEnabled: true, Variant: "control", Payload: null);

        var result = AbTest.Run(flag, f => f.Variant);

        Assert.Equal("control", result);
    }

    [Fact]
    public void Run_FuncWithFlag_FlagDisabled_ReturnsDefault()
    {
        var flag = new FlagResult(IsEnabled: false, Variant: "control", Payload: null);

        var result = AbTest.Run(flag, f => f.Variant);

        Assert.Null(result);
    }

    [Fact]
    public void Run_FuncWithFlag_NullFlag_ReturnsDefault()
    {
        var result = AbTest.Run<string>(null, f => f.Variant);

        Assert.Null(result);
    }

    // AbTest.Run (non-returning, with FlagResult, with payload)

    [Fact]
    public void Run_ActionWithPayloadFlag_FlagEnabled_UnwrapsPayload()
    {
        dynamic expando = new ExpandoObject();
        expando.proficiency = 0.1;

        double proficiency = 0.0;

        var flag = new FlagResult(IsEnabled: true, Variant: null, Payload: new SafePayload(expando));

        AbTest.Run(flag, f => proficiency = f.Payload!.proficiency );

        Assert.Equal(0.1, proficiency);
    }
}