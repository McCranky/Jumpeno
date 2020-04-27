using Jumpeno.Data;
using Jumpeno.JumpenoComponents.Entities;
using Jumpeno.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace Jumpeno.JumpenoComponents.Editors {
    /**
     * Slúži ako základná trieda pre komponent SkinEdit.razor.
     * Umožňuje prehliadať a meniť skiny hráča.
     */
    public class SkinEditorBase : ComponentBase {
        [Inject] protected LocalStorageTrackingService LocalStorage { get; set; }
        [Inject] protected UserManager<JumpenoUser> UserManager { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> AuthState { get; set; }
        private JumpenoUser activeUser;
        public Dictionary<string, Animation> Skins { get; set; }
        public string SkinName { get; set; } = "";
        private int imageFrame = 0;
        private System.Timers.Timer timer;
        public SkinEditorBase() {
            Skins = new Dictionary<string, Animation>(5);
            Vector2 bodySize = new Vector2(0,0);
            Skins.Add("fire", new Animation("mageSprite_fire.png", new Vector2(4, 3), out bodySize));
            Skins.Add("aer", new Animation("mageSprite_aer.png", new Vector2(4, 3), out bodySize));
            Skins.Add("earth", new Animation("mageSprite_earth.png", new Vector2(4, 3), out bodySize));
            Skins.Add("water", new Animation("mageSprite_water.png", new Vector2(4, 3), out bodySize));
            Skins.Add("magic", new Animation("mageSprite_magic.png", new Vector2(4, 3), out bodySize));

            timer = new System.Timers.Timer(10000.0 / 60);
            timer.Elapsed += async (sender, e) => await Tick(sender, e);
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                var auth = await AuthState;
                activeUser = await UserManager.GetUserAsync(auth.User);

                SkinName = activeUser.Skin.Substring(activeUser.Skin.LastIndexOf("_") + 1);
            }
        }

        private async Task Tick(Object source, ElapsedEventArgs e) {
            ++imageFrame;
             await InvokeAsync(StateHasChanged);
        }

        protected async Task ChooseSkin() {
            activeUser.Skin = "mageSprite_" + SkinName;
            await UserManager.UpdateAsync(activeUser);
        }

        protected void OnSkinSelection(string name) {
            SkinName = name;
        }

        protected string GetCurrentFrameStyle(State state) {
            if (String.IsNullOrEmpty(SkinName)) {
                return "";
            }
            return Skins[SkinName].GetFrameStyle(state, imageFrame);
        }
        
        protected string GetFirstFrameStyle(State state) {
            if (String.IsNullOrEmpty(SkinName)) {
                return "";
            }
            if (state == State.FALLING) {
                return Skins[SkinName].GetFrameStyle(state, 1);
            } else if (state == State.DEAD) {
                return Skins[SkinName].GetFrameStyle(State.FALLING, 0);
            }
            return Skins[SkinName].GetFrameStyle(state, 0);
        }
    }
}
