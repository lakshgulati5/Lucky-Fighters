﻿namespace Lucky_Fighters
{
    public class Shield : Interactive
    {
        public Shield() : base("Shield", 0, false)
        {
        }

        internal override void ApplyEffect(Player player)
        {
            if (!IsEnabled) return;
            
            player.AddTask(
                new Task(0, () =>
                    {
                        player.IsShielded = true;
                        IsEnabled = false;
                    }
                ).Then(5, () => { player.IsShielded = false; })
            );
        }
    }
}