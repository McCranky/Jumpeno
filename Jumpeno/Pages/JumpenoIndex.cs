﻿using Jumpeno.JumpenoComponents;
using Jumpeno.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Jumpeno.Data;
using Jumpeno.JumpenoComponents.Entities;
using Jumpeno.JumpenoComponents.Game;
using Jumpeno.JumpenoComponents.Utilities;

namespace Jumpeno.Pages
{
    public class JumpenoIndexBase : ComponentBase, IDisposable
    {
        //### Injecting section ----------------------------------------------------
        [Inject] protected LocalStorageTrackingService LocalStorage { get; set; }
        [Inject] protected NavigationManager Navigation { get; set; }
        [Inject] protected MatchController MatchController { get; set; }
        [Inject] protected Player Player { get; set; }
        [Inject] protected UserManager<JumpenoUser> UserManager { get; set; }
        [Inject] protected IJSRuntime JsRuntime { get; set; }


        //### Variables section ---------------------
        private Game game;
        private readonly Random rnd = new Random();
        protected InitialisingInfo InitInfo = new InitialisingInfo();
        protected List<JumpenoComponent> VisibleComponents = new List<JumpenoComponent>();


        //### Parameters section --------------------------
        [CascadingParameter] Task<AuthenticationState> authenticationStateTask { get; set; }
        [Parameter]
        public string GameCode { 
            get { 
                return InitInfo.GameCode; 
            } 
            set { 
                InitInfo.GameCode = value; 
            } 
        }


        //### Property section ----------------------
        public Game Game {
            get { return game; }
            set { this.game = value; }
        }
        public float Width { get; set; }
        public float Height { get; set; }
        protected string LoginMethod { get; set; }


        //### Methods section ----------------------------------------------
        [JSInvokable]
        public async Task OnBrowserResize() {
            var visibleArea = await JsRuntime.InvokeAsync<long[]>("GetSize");
            Width = visibleArea[0];
            Height = visibleArea[1];
            if (Width < 1050) {
                if (Player != null) {
                    Player.SmallScreen = true;
                }
            } else {
                if (Player != null) {
                    Player.SmallScreen = false;
                }
            }
            StateHasChanged();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await JsRuntime.InvokeAsync<object>("WindowResized", DotNetObjectReference.Create(this));
                await OnBrowserResize();

                LoginMethod = await LocalStorage.GetItemValue(LocalStorageTrackingService.Item.PLAYER_LOGIN_METHOD);
                string code = await LocalStorage.GetItemValue(LocalStorageTrackingService.Item.GAME_CODE);
                var user = (await authenticationStateTask).User;

                if (user.Identity.IsAuthenticated) {
                    var activeUser = await UserManager.GetUserAsync(user);
                    Player.Name = activeUser.UserName;
                    Player.Skin = activeUser.Skin;
                    await LocalStorage.SetItemValue(LocalStorageTrackingService.Item.PLAYER_NAME, Player.Name);
                    await LocalStorage.SetItemValue(LocalStorageTrackingService.Item.PLAYER_SKIN, activeUser.Skin);
                    await LocalStorage.SetItemValue(LocalStorageTrackingService.Item.PLAYER_LOGIN_METHOD, LocalStorageTrackingService.LogInMethod.IDENTITY.ToString());
                } else {
                    if (String.IsNullOrEmpty(LoginMethod)) {
                        if (!String.IsNullOrEmpty(GameCode)) {
                            await LocalStorage.SetItemValue(LocalStorageTrackingService.Item.GAME_CODE, GameCode);
                        }
                        Navigation.NavigateTo("/redirection");
                        return;
                    } else if (LoginMethod == LocalStorageTrackingService.LogInMethod.ANONYM.ToString()) {
                        Player.Name = await LocalStorage.GetItemValue(LocalStorageTrackingService.Item.PLAYER_NAME);
                        Player.Skin = await LocalStorage.GetItemValue(LocalStorageTrackingService.Item.PLAYER_SKIN);
                    } else if (LoginMethod == LocalStorageTrackingService.LogInMethod.SPECTATOR.ToString()) {
                        if (String.IsNullOrEmpty(code)) {
                            Player.Spectator = false;
                            await LocalStorage.RemoveItemValue(LocalStorageTrackingService.Item.PLAYER_LOGIN_METHOD);
                            Navigation.NavigateTo("/redirection");
                            return;
                        } else {
                            Player.Spectator = true;
                        }                        
                    }
                }

                
                if (code == null) {
                    if (GameCode != null) { // hrac sa pripaja pomocou odkazu/QRkodu
                        JoinGame(false);
                    } else { // hrač sa pripaja pomocou kodu alebo je po prvýkrát na stránke

                    }
                } else { // pouzivatel refreshol stranku alebo prišiel z prihlasenia
                    GameCode = code;
                    JoinGame(true);
                }
                StateHasChanged();
            }
        }

        public void Dispose() {
            LeaveLobby();
        }

        private void LeaveGame() {
            if (Game != null) {
                Game.OnTickReached -= UpdateUi;
                Task.Run(async () => {
                    await LocalStorage.RemoveItemValue(LocalStorageTrackingService.Item.GAME_CODE);
                });
                GameCode = null;
            }
        }

        protected void DeleteGame() {
            Game.GameState = GameState.DELETED;
            LeaveGame();
            MatchController.DeleteGame(Game.Code);
            Game = null;
            Navigation.NavigateTo(Navigation.BaseUri);
        }

        protected int GetProgressBar() {
            return (int)Math.Ceiling(((double)Game.PlayersInGame.Count / Game.PlayersLimit) * 100.0);
        }

        protected string GetUrl() {
            return Navigation.BaseUri + Game.Code;
        }

        protected void LeaveLobby(bool invokeByPlayer = false) {
            if (Player == Game?.Creator && Game.GameMode == GameMode.GUIDED) {
                DeleteGame();
                return;
            }
            Game?.RemovePlayer(Player);
            LeaveGame();
            Game = null;
            if (invokeByPlayer) {
                Navigation.NavigateTo(Navigation.BaseUri);
            }
        }

        protected void CreateGame() {
            if (MatchController.TryAddGame(InitInfo, out game)) {
                OnLobbyJoin(false);
                if (InitInfo.GameMode != GameMode.GUIDED) {
                    Game.AddPlayer(Player);
                } else {
                    Game.Creator = Player;
                    Player.InGame = true;
                }
            } else {
                InitInfo.ErrorMessage = "Game could not be created.";
            }
        }

        protected void JoinGame(bool alreadyIn) {
            if (!MatchController.TryGetGame(GameCode, out game)) {
                InitInfo.ErrorMessage = "Game with this code does not exist.";
            } else {
                if (!Player.Spectator) {
                    bool result;
                    lock (Game) {
                        Player tmp = Game.GetPlayer(Player.Name);
                        if (tmp == null) {
                            result = Game.AddPlayer(Player);
                            if (!result) {
                                InitInfo.ErrorMessage = "Lobby is full.";
                                game = null;
                                return;
                            }
                        } else {
                            Player = tmp;
                        }
                    }
                } else {
                    // in case some spectator limitation
                }
                OnLobbyJoin(alreadyIn);
            }
        }

        protected void StartGame() {
            Game.Start();
        }
        
        protected async void OnLobbyJoin(bool alreadyIn) {
            if (!alreadyIn) {
                await LocalStorage.SetItemValue(LocalStorageTrackingService.Item.GAME_CODE, GameCode);
            }
            if (Game.GameMode == GameMode.GUIDED) {
                Player.Name = Player._UserNames[rnd.Next(0, Player._UserNames.Length)];
            } else {
                Player.Name = await LocalStorage.GetItemValue(LocalStorageTrackingService.Item.PLAYER_NAME);
            }
            Game.OnTickReached += UpdateUi; // each time the game is updated, the browser window is also updated
            Navigation.NavigateTo(Navigation.BaseUri + GameCode);
        }

        private void UpdateUi(object sender, EventArgs ea) {
            if (Game?.GameState == GameState.DELETED) {
                LeaveLobby(true);
                InvokeAsync(StateHasChanged);
            } else {
                InvokeAsync(
                () => {
                    if (Game != null) {
                        lock (Game.PlayersInGame) {
                            if (Game != null) {
                                VisibleComponents = Game.PlayersInGame.Where(player => player.Visible).ToList().ConvertAll(player => (JumpenoComponent)player);
                                if (Game.GameState == GameState.COUNTDOWN ||
                                    Game.GameState == GameState.SHRINKING ||
                                    Game.GameState == GameState.GAMEOVER) {
                                    VisibleComponents.AddRange(Game.Map.Platforms.Where(tile => tile.Visible));
                                }
                            }
                            StateHasChanged();
                        }
                    } else {
                        StateHasChanged();
                    }
                });
            }
            //DEBUG components updated
            //System.Console.WriteLine($"VisibleComponents updated!");
        }

        protected void KeyDown(KeyboardEventArgs e) {
            switch (e.Key) {
                case "ArrowRight":
                    Player.SetMovement(MovementAction.RIGHT, true);
                    break;
                case "ArrowLeft":
                    Player.SetMovement(MovementAction.LEFT, true);
                    break;
                case " ":
                    Player.SetMovement(MovementAction.JUMP, true);
                    break;
            }
        }

        protected void KeyUp(KeyboardEventArgs e) {
            switch (e.Key) {
                case "ArrowRight":
                    Player.SetMovement(MovementAction.RIGHT, false);
                    break;
                case "ArrowLeft":
                    Player.SetMovement(MovementAction.LEFT, false);
                    break;
                case " ":
                    Player.SetMovement(MovementAction.JUMP, false);
                    break;
            }
        }

        protected void Respawn() {
            Game.RespawnPlayer(Player);
        }
    }
}
