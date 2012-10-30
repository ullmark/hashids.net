using System.Linq;
using System.Reflection;
using NSpec;
using NSpec.Domain;

/*
 * Howdy,
 * 
 * This is NSpec's DebuggerShim.  It will allow you to use TestDriven.Net or Resharper's test runner to run
 * NSpec tests.  
 * 
 * It's DEFINITELY worth trying specwatchr (http://nspec.org/continuoustesting). Specwatchr automatically
 * runs tests for you.
 * 
 * If you ever want to debug a test when using Specwatchr, simply put the following line in your test:
 * 
 *     System.Diagnostics.Debugger.Launch()
 *     
 * Visual Studio will detect this and will give you a window which you can use to attach a debugger.
 */

//[TestFixture]
public class DebuggerShim
{
    //[Test]
    public void debug()
    {
        var tagOrClassName = "class_or_tag_you_want_to_debug";

        var invocation = new RunnerInvocation(Assembly.GetExecutingAssembly().Location, tagOrClassName);

        var contexts = invocation.Run();

        //assert that there aren't any failures
        contexts.Failures().Count().should_be(0);
    }
}
