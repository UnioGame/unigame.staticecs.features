namespace unigame.staticecs.features {
    public enum AbilityChangeReason : byte {
        Started = 0,
        PhaseAdvanced = 1,
        Completed = 2,
        Cancelled = 3,
        Interrupted = 4,
    }
}
