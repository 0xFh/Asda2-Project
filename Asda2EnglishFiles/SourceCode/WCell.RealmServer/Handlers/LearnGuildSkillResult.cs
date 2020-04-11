namespace WCell.RealmServer.Handlers
{
    public enum LearnGuildSkillResult
    {
        Failed,
        Ok,
        ProblemInUserProfile,
        YouAreNotInAGuild,
        ProblemWithGuildInfo,
        YouDontHavePermitionToDoThis,
        ProlemWithSkillInfo,
        ThisIsTheMaxLevelOfSkill,
        GuildLevelIsNotEnoght,
        IncifitientPoints,
        CantLevelupCurrentActivatedSkills,
    }
}