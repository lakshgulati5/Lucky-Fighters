namespace Lucky_Fighters
{
    public class Wings : Interactive
    {
        public Wings() : base("Wings", 0, false)
        {
        }

        internal override void ApplyEffect(Player player)
        {
            if (!IsEnabled) return;
            
            player.AddTask(
                new Task(0, () =>
                    {
                        player.HasWings = true;
                        IsEnabled = false;
                    }
                ).Then(10, () => { player.HasWings = false; })
            );
        }
    }
}