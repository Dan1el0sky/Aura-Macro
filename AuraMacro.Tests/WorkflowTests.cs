using System.Threading.Tasks;
using Xunit;
using AuraMacro.Core.Engine;
using AuraMacro.Core.Mocks;
using AuraMacro.Core.Models;
using AuraMacro.Core.Workflow;
using System.IO;

namespace AuraMacro.Tests
{
    public class WorkflowTests
    {
        [Fact]
        public void SerializeDeserialize_PreservesActionsAndPolymorphism()
        {
            // Arrange
            var wf = new MacroWorkflow { Name = "Test" };
            wf.Actions.Add(new ClickAction { Id = "1", X = 50, Y = 50 });
            wf.Actions.Add(new WaitForTextAction { Id = "2", TextToFind = "Hello", SaveToVariable = "var1" });
            wf.Actions.Add(new IfElseAction { Id = "3", VariableName = "var1", ExpectedValue = "Hello" });

            // Act
            string json = WorkflowSerializer.Serialize(wf);
            var deserialized = WorkflowSerializer.Deserialize(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("Test", deserialized!.Name);
            Assert.Equal(3, deserialized.Actions.Count);

            Assert.IsType<ClickAction>(deserialized.Actions[0]);
            Assert.IsType<WaitForTextAction>(deserialized.Actions[1]);
            Assert.IsType<IfElseAction>(deserialized.Actions[2]);

            var click = (ClickAction)deserialized.Actions[0];
            Assert.Equal(50, click.X);

            var waitForText = (WaitForTextAction)deserialized.Actions[1];
            Assert.Equal("Hello", waitForText.TextToFind);
        }

        [Fact]
        public async Task MacroRunner_IfElse_BranchesCorrectly()
        {
            // Arrange
            var ocrEngine = new MockOcrEngine { MockedResult = "YES" };
            var simulator = new MockInputSimulator();
            var runner = new MacroRunner(simulator, ocrEngine);

            var wf = new MacroWorkflow();

            wf.Actions.Add(new WaitForTextAction
            {
                Id = "step1", TextToFind = "Y", SaveToVariable = "result"
            });

            wf.Actions.Add(new IfElseAction
            {
                Id = "step2",
                VariableName = "result",
                ExpectedValue = "YES",
                OnTrueGotoId = "step_true",
                OnFalseGotoId = "step_false"
            });

            // True branch
            wf.Actions.Add(new KeyPressAction { Id = "step_true", Key = "TRUE_KEY" });

            // To prevent falling through to FALSE_KEY sequentially after TRUE_KEY,
            // we should technically have a Stop/End action, or just put them in a way
            // where FALSE branch jumps somewhere else or TRUE branch jumps to end.
            // For testing the basic branching logic, I'll just change the workflow structure:

            // False branch
            wf.Actions.Add(new KeyPressAction { Id = "step_false", Key = "FALSE_KEY" });

            // To avoid sequential fall-through from step_true to step_false,
            // I'll make TRUE branch the last item in the list, and FALSE branch before it.
            // Wait, list order determines sequential fallback. Let's reorder:

            var wf2 = new MacroWorkflow();
            wf2.Actions.Add(new WaitForTextAction { Id = "s1", TextToFind = "Y", SaveToVariable = "res" });
            wf2.Actions.Add(new IfElseAction { Id = "s2", VariableName = "res", ExpectedValue = "YES", OnTrueGotoId = "true", OnFalseGotoId = "false" });
            wf2.Actions.Add(new KeyPressAction { Id = "false", Key = "FALSE_KEY" });
            wf2.Actions.Add(new KeyPressAction { Id = "true", Key = "TRUE_KEY" });

            var sw = new StringWriter();
            var originalOut = System.Console.Out;
            System.Console.SetOut(sw);

            try
            {
                // Act
                await runner.RunAsync(wf2);

                // Assert
                var output = sw.ToString();
                Assert.Contains("[MOCK] KeyPress simulated: TRUE_KEY", output);
                Assert.DoesNotContain("[MOCK] KeyPress simulated: FALSE_KEY", output);
            }
            finally
            {
                System.Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void ActionRecorder_RecordsCorrectly()
        {
            var mockHook = new MockInputHook();
            var recorder = new ActionRecorder(mockHook);

            recorder.Start();
            mockHook.SimulateClick(100, 200);
            mockHook.SimulateKeyPress("A");
            var actions = recorder.Stop();

            Assert.Equal(2, actions.Count);
            Assert.IsType<ClickAction>(actions[0]);
            Assert.Equal(100, ((ClickAction)actions[0]).X);
            Assert.IsType<KeyPressAction>(actions[1]);
            Assert.Equal("A", ((KeyPressAction)actions[1]).Key);
        }
    }
}