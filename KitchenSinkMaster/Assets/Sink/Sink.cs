class Sink : SinkBase {
    private static int moduleIDCounter = 1;
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = SinkHelpMessage;
    #pragma warning restore 414
    protected override void SetFields()
    {
        _moduleId = moduleIDCounter++;
        _selectableChildren = new[] { Cold, Hot };
    }
}
