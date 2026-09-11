using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mandible.Core.Data;

public interface IInputSystem
{
    void Update();
    void Activate(string actionName);
    bool WasActivatedThisFrame(string actionName);

    //API
    InputSignal GetSignal(string actionName);
    public bool Consume(string actionName, InputType type);
    public bool ConsumePressed(string action);
    public bool ConsumeHeld(string action);
    public bool ConsumeReleased(string action);
    public bool Pressed(string action);
    public bool Held(string action);
    public bool Released(string action);

    //Advanced
    T GetContext<T>(string actionName) where T : struct;
}