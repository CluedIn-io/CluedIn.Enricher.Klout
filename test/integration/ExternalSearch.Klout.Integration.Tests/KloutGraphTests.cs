namespace ExternalSearch.Klout.Integration.Tests
{
    public class KloutTests
    {
        // TODO Issue 170 - Test Failures
        //[Fact]
        //public void Test()
        //{
        //    // Arrange
        //    this.testContext = new TestContext();
        //    var properties = new EntityMetadataPart();
        //    properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInUser.SocialTwitter, "stephenpope");
        
        //    IEntityMetadata entityMetadata = new EntityMetadataPart()
        //        {
        //            Name        = "Stephen Pope",
        //            EntityType  = EntityType.Person,
        //            Properties = properties.Properties
        //        };

        //    var externalSearchProvider  = new Mock<KloutExternalSearchProvider>(MockBehavior.Loose);
        //    var clues                   = new List<CompressedClue>();

        //    externalSearchProvider.CallBase = true;

        //    this.testContext.ProcessingHub.Setup(h => h.SendCommand(It.IsAny<ProcessClueCommand>())).Callback<IProcessingCommand>(c => clues.Add(((ProcessClueCommand)c).Clue));
        //    this.testContext.Container.Register(Component.For<IExternalSearchProvider>().UsingFactoryMethod(() => externalSearchProvider.Object));

        //    var context         = this.testContext.Context.ToProcessingContext();
        //    var command         = new ExternalSearchCommand();
        //    var actor           = new ExternalSearchProcessing(context.ApplicationContext);
        //    var workflow        = new Mock<Workflow>(MockBehavior.Loose, context, new EmptyWorkflowTemplate<ExternalSearchCommand>());
            
        //    workflow.CallBase = true;

        //    command.With(context);
        //    command.OrganizationId  = context.Organization.Id;
        //    command.EntityMetaData  = entityMetadata;
        //    command.Workflow        = workflow.Object;
        //    context.Workflow        = command.Workflow;

        //    // Act
        //    var result = actor.ProcessWorkflowStep(context, command);
        //    Assert.Equal(WorkflowStepResult.Repeat.SaveResult, result.SaveResult);

        //    result = actor.ProcessWorkflowStep(context, command);
        //    Assert.Equal(WorkflowStepResult.Success.SaveResult, result.SaveResult);
        //    context.Workflow.AddStepResult(result);
            
        //    context.Workflow.ProcessStepResult(context, command);

        //    // Assert
        //    this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);

        //    Assert.True(clues.Count > 0);
        //}
    }
}
