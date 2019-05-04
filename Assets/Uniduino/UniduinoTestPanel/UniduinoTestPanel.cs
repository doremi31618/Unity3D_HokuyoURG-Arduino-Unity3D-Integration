using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Uniduino;
using Uniduino.Helpers;
using UnityEngine.UI;

#if (UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)        
public class UniduinoTestPanel : Uniduino.Examples.UniduinoTestPanel { }
#endif


namespace Uniduino.Examples
{
    
    /// <summary>
    /// Uniduino TestPanel - a GUI tool for easy visualization and manipulation of I/O functions
    /// </summary>

    public class UniduinoTestPanel : MonoBehaviour {
    
    
        public GUISkin skin;
        public Arduino arduino;
        private float _loopTime = 1;

        public float loopTime
        {
            get
            {
                return _loopTime;
            }
            set
            {
                _loopTime = value;
            }
        }
        public float shiningTime = 0.5f;

        [Header("AnimationSystem")]
        [Header("state setting")]
        public int frameRate = 5;
        public bool isLoop = true;
        public bool isUseGlobalAntiLogic = false;

        [Header("Animation clip setting")]
        public List<AnimationModel> animationClips;

        [Header("Simulation GameObject")]
        public List<GameObject> Light;
        private bool[] LightStates;

        public AnimationType nowPlaying;
            
        void Start () {        
            
            if (!UniduinoSetupHelpers.SerialPortAvailable())
            {
    #if UNITY_EDITOR
                    
                UniduinoSetupHelpers.OpenSetupWindow();    
    #else
                Debug.LogError("Uniduino SerialPort Support must be installed: is libMonoPosixHelper on your path?");
    #endif
            }
            
            arduino = Arduino.global;                         // convenience, alias the global arduino singleton
            arduino.Log = (s) => Debug.Log("Arduino: " +s); // Attach arduino logs to Debug.Log
            hookEvents();                                    // set up event callbacks for received data

        }

        
        protected List<Arduino.Pin> received_pins;
        
        protected void hookEvents()
        {
            arduino.AnalogDataReceived += delegate(int pin, int value) 
            {
                Debug.Log("Analog data received: pin " + pin.ToString() + "=" + value.ToString());                            
            };
        
            arduino.DigitalDataReceived += delegate(int portNumber, int portData)                     
            {
                Debug.Log("Digital data received: port " + portNumber.ToString() + "=" + System.Convert.ToString(portData, 2));                            
            };
        
            arduino.VersionDataReceived += delegate(int majorVersion, int minorVersion) 
            {
                Debug.Log("Version data received");
                arduino.queryCapabilities();
            };
        
            arduino.CapabilitiesReceived += delegate(List<Arduino.Pin> pins)
            {
                Debug.Log("Pin capabilities received");
                received_pins = pins; // cache the complete pin list here so we can use it without worrying about it being complete yet
                
            };
        
            //arduino.reportVersion(); // some boards (like the micro) do not send the version right away for some reason. perhaps a timing issue.        
            
        }
        
        protected void connect()
        {
            arduino.Connect();
        
        }
        public int[] LedPin;
        public int pinNumber = 1;
        void ConfigurePins()
        {
            //arduino.pinMode(13, PinMode.OUTPUT);
            //arduino.digitalWrite(13, Arduino.HIGH);
            Debug.Log("Configure Pins");
            //arduino.Baud = Baud;
            for (int i = 0; i < LedPin.Length; i++)
            {
                arduino.pinMode(LedPin[i], PinMode.OUTPUT);
                arduino.digitalWrite(LedPin[i], Arduino.HIGH);
            }
        }

        //arduino function
        public IEnumerator RelayControll()
        {
            /*
            Debug.Log("Configure Pins");
            //arduino.Baud = Baud;

            for (int i = 0; i < LedPin.Length; i++)
            {
                arduino.pinMode(LedPin[i], PinMode.OUTPUT);
                arduino.digitalWrite(LedPin[i], Arduino.LOW);
            }

            int currentPin = 0;
            Debug.Log("Relay Control");
            //arduino.Baud = Baud;
            int arduinoState1 = 0;
            int arduinoState2 = 0;
            */
            //arduinoState1 default value equal to high
            //arduinoState2 default value equal to low
            int arduinoState1 = LogicStateSetting.State1 ^ isUseGlobalAntiLogic ? Arduino.HIGH : Arduino.LOW;
            int arduinoState2 = LogicStateSetting.State2 ^ isUseGlobalAntiLogic ? Arduino.LOW : Arduino.HIGH;

            //light animation attribute initialize
            int playingIndex = 0;
            nowPlaying = animationClips[playingIndex].type;
            LightStates = new bool[m_Urg.getDetecRegions.Length];

            //float[] LEDTimer = new float[m_Urg.getDetecRegions.Length];

            //loop function
            while (isLoop)
            {/*
                int nextPin = CalculateNextLedPin(currentPin);
                Debug.Log("next Pin : " + LedPin[nextPin]);

                //turn of the Led
                arduino.digitalWrite(LedPin[nextPin], Arduino.LOW);

                Debug.Log("current pings : " + LedPin[currentPin]);

                //turn on the Led
                arduino.digitalWrite(LedPin[currentPin], Arduino.HIGH);

                //reset the data
                currentPin = nextPin;
                */

                
                //check what stat now it is 
                animationClips[playingIndex].AnimationSetting.stateSelector();
                switch (animationClips[playingIndex].m_animation.nowState)
                {
                    case AnimationPrototype.animationState.start:
                        animationClips[playingIndex].AnimationSetting.InitializeAnimation(ref LightStates);
                        break;
                    case AnimationPrototype.animationState.update:
                        animationClips[playingIndex].m_animation.UpdateAnimation(ref LightStates);
                        break;
                    case AnimationPrototype.animationState.end:
                        animationClips[playingIndex].m_animation.EndAnimation(ref LightStates);
                        playingIndex += 1;
                        if (playingIndex >= animationClips.Count)
                        {
                            playingIndex = 0;
                        }
                        nowPlaying = animationClips[playingIndex].type;
                        break;
                }

                for (int i = 0; i < m_Urg.getDetecRegions.Length; i++)
                {
                    var state = arduino.digitalRead(LedPin[i]);
                    var boolTransferToArduinoState = LightStates[i] ^ isUseGlobalAntiLogic ? Arduino.HIGH : Arduino.LOW;
                    Debug.Log("Led pin : " + LedPin[i] + " State : " + state + " Light state : " + boolTransferToArduinoState);
                    if (state != boolTransferToArduinoState)
                    {
                        //Light[i].GetComponent<MeshRenderer>().enabled = LightStates[i] ^ isUseGlobalAntiLogic;
                        Debug.Log("Led pin : " + LedPin[i] + "Arduino write: " + boolTransferToArduinoState);
                        arduino.digitalWrite(LedPin[i], boolTransferToArduinoState);
                    }
                }

                //delay 1 second
                yield return new WaitForSeconds(1 / frameRate);

            }

        }
        public Text infoText;
        public URGSample m_Urg;   
        //arduino function
        public IEnumerator URGControl()
        {

            //arduino.Baud = Baud;
            int arduinoState1 = 0;
            int arduinoState2 = 0;

            //arduinoState1 default value equal to high
            //arduinoState2 default value equal to low
            arduinoState1 = LogicStateSetting.State1 ^ isUseGlobalAntiLogic ? Arduino.HIGH : Arduino.LOW;
            arduinoState2 = LogicStateSetting.State2 ^ isUseGlobalAntiLogic ? Arduino.LOW : Arduino.HIGH;

            //light animation attribute initialize
            int playingIndex = 0;
            nowPlaying = animationClips[playingIndex].type;
            LightStates = new bool[m_Urg.getDetecRegions.Length];

            float[] LEDTimer = new float[m_Urg.getDetecRegions.Length];
            //Debug.Log("getDetecRegion : " + m_Urg.getDetecRegions.Length);
            for (int i = 0; i < LedPin.Length; i++) 
            {
                
                arduino.pinMode(LedPin[i], PinMode.OUTPUT);
                arduino.digitalWrite(LedPin[i], (arduinoState2));

                LEDTimer[i] = 0;
            }

           
            //loop function
            while (true)
            {
                if (m_Urg.urg.isConnected)
                {
                    infoText.text = "";

                    //event check layer
                    if(!isDetectNothing() && isLoop)
                    {
                        isLoop = false;
                        playingIndex = 0;
                        nowPlaying = animationClips[playingIndex].type;
                        LightStates = new bool[m_Urg.getDetecRegions.Length];
                    }
                    else if(isDetectNothing() && !isLoop)
                    {
                        isLoop = true;
                        playingIndex = 0;
                        nowPlaying = animationClips[playingIndex].type;
                        LightStates = new bool[m_Urg.getDetecRegions.Length];
                    }

                    //playing animation
                    if(isDetectNothing() && isLoop)
                    {
                        
                            //check what stat now it is 
                            animationClips[playingIndex].AnimationSetting.stateSelector();
                            switch (animationClips[playingIndex].m_animation.nowState)
                            {
                                case AnimationPrototype.animationState.start:
                                    animationClips[playingIndex].AnimationSetting.InitializeAnimation(ref LightStates);
                                    break;
                                case AnimationPrototype.animationState.update:
                                    animationClips[playingIndex].m_animation.UpdateAnimation(ref LightStates);
                                    break;
                                case AnimationPrototype.animationState.end:
                                    animationClips[playingIndex].m_animation.EndAnimation(ref LightStates);
                                    playingIndex += 1;
                                    if (playingIndex >= animationClips.Count)
                                    {
                                        playingIndex = 0;
                                    }
                                    nowPlaying = animationClips[playingIndex].type;
                                    break;
                            }
                            /*
                            for (int i = 0; i < m_Urg.getDetecRegions.Length; i++)
                            {
                                var state = arduino.digitalRead(LedPin[i]);
                                var boolTransferToArduinoState = LightStates[i] ^ isUseGlobalAntiLogic ? Arduino.HIGH : Arduino.LOW;
                                //Debug.Log("Led pin : " + LedPin[i] + " State : " + state + " Light state : " + boolTransferToArduinoState);
                                if (state != boolTransferToArduinoState)
                                {
                                    
                                    Light[i].GetComponent<MeshRenderer>().enabled = LightStates[i] ^ isUseGlobalAntiLogic;
                                    //Debug.Log("Led pin : " + LedPin[i] + "Arduino write: " + boolTransferToArduinoState);
                                    arduino.digitalWrite(LedPin[i], boolTransferToArduinoState);
                                }
                            }
                            */
                            for (int i = 0; i < Light.Count; i++)
                            {
                                if (Light[i].GetComponent<MeshRenderer>().enabled != LightStates[i] ^ isUseGlobalAntiLogic)
                                {
                                    Light[i].GetComponent<MeshRenderer>().enabled = LightStates[i] ^ isUseGlobalAntiLogic;
                                var boolTransferToArduinoState = LightStates[i] ^ isUseGlobalAntiLogic ? Arduino.HIGH : Arduino.LOW;
                                arduino.digitalWrite(LedPin[i], boolTransferToArduinoState);
                            }
                                    //Light[i].GetComponent<MeshRenderer>().enabled = LightStates[i] ^ isUseGlobalAntiLogic;
                            }
                        //yield return new WaitForSeconds(1 / frameRate);

                    }
                    else
                    {
                        for (int i = 0; i < m_Urg.getDetecRegions.Length; i++)
                        {
                            var state = arduino.digitalRead(LedPin[i]);
                            //infoText.text +="DetectRegion : " + m_Urg.getDetecRegions[i] + " LedPin : " + LedPin[i] + " state : " + state + "\n"; 
                            if (m_Urg.getDetecRegions[i] && state != arduinoState2)
                            {
                                state = arduinoState2;
                            }
                            else if(!m_Urg.getDetecRegions[i] && state == arduinoState2)
                            {
                                state = arduinoState1;
                            }
                            arduino.digitalWrite(LedPin[i], state);
                            infoText.text += "DetectRegion : " + m_Urg.getDetecRegions[i] + " LedPin : " + LedPin[i] + " state : " + state + "\n"; 
                        }
                    }

                    yield return new WaitForSeconds(1 / frameRate);
                }
                else
                {
                    //Debug.LogError("HOKUYO not connect");
                    yield return new WaitForSeconds(3f);
                }

            }

        }
        bool isDetectNothing()
        {
            int counter = 0;

            for (int i = 0; i < LedPin.Length; i++)
            {
                if (!m_Urg.getDetecRegions[i])
                {
                    counter += 1;
                }
            }
            bool result = (counter == LedPin.Length);
            return result;
        }

        int CalculateNextTwoLedPin(int LED1, int LED2,int LastLED2,int count)
        {
            LED1 = LastLED2;
            LED2 = LED1 + 1;
            if (LED2 > count)
                LED2 = 0;
            LastLED2 = LED2;
            return LastLED2;
        }

        int CalculateNextLedPin(int currentLedPin)
        {
            int NextLedPin = 0;

            if (currentLedPin == LedPin.Length - 1)
                NextLedPin = 0;
            else
                NextLedPin = currentLedPin + 1;

            return NextLedPin;
        }

        Coroutine blinker;
        int label_column_width = 100;
        int test_column_width = 70;
        Vector2 scroll_position = new Vector2(0,0);    
        void OnGUI()
        {
            GUI.skin = skin;
            
            GUILayout.BeginArea(new Rect(50, 50, 2*Screen.width/2-100, Screen.height-100));                
            {
            
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                            
                            if (GUILayout.Button("Connect"))
                            {
                                connect();
                            }                    
                            
                            if (GUILayout.Button("Disconnect"))
                            {
                                Debug.Log ("Closing connection to arduino");        
                                //try {                        
                                    arduino.Disconnect();
                                /*} catch ( Exception e)
                                {
                                    Debug.Log("Exception initializing arduino:" + e.ToString());
                                }*/
                                received_pins = null;
                            }    
                            if (GUILayout.Button("Relay Animation"))
                            {
                                Debug.Log("Start coroutine");
                                StartCoroutine(RelayControll());
                            }

                            if (GUILayout.Button("URGControl"))
                            {
                                Debug.Log("Start coroutine URGControl");
                                StartCoroutine(URGControl());
                            }

                            GUILayout.FlexibleSpace();
                            
                            GUILayout.Label("Serial port:");
                            arduino.PortName = GUILayout.TextField(arduino.PortName);
                            if (GUILayout.Button("Guess"))
                            {
                                string pn = Arduino.guessPortName();                    
                                if (pn.Length > 0) arduino.PortName = pn;
                            }                
                            
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            string connection_status;
                            if (arduino != null && arduino.IsOpen && arduino.Connected)
                            {
                                connection_status = "Connected to Firmata protocol version " + arduino.MajorVersion + "." + arduino.MinorVersion;
                                GUILayout.Label(connection_status);
                                
                            } else if (arduino != null && arduino.IsOpen)
                            {
                                GUILayout.Label("Connected but waiting for Firmata protocol version" );
                            } else
                            {
                                GUILayout.Label("Not connected");    
                            }
                                    
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Query capabilities"))
                            {
                                arduino.queryCapabilities();                        
                            }
                        }
                        GUILayout.EndHorizontal();
                        
                        GUILayout.Space(20);
                        
                        // draw pin states and controls
                        if (received_pins != null) //arduino != null && arduino.Pins != null)
                        {                    
                            
                            GUILayout.BeginHorizontal();
                            {
                                GUIStyle style = new GUIStyle(GUI.skin.label);
                                
                                style.fontSize +=3;
                                style.fontStyle = FontStyle.Bold;
                                
                                GUILayout.Label("Pin:Value", style, GUILayout.Width(label_column_width));                        
                                GUILayout.Label("Output", style, GUILayout.Width(test_column_width));
                                GUILayout.Label("Modes",style, GUILayout.Width(label_column_width)); // these widths hacked due to awful unity bug in label size calculations
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("Reporting",style, GUILayout.MinWidth(style.CalcSize(new GUIContent("Reporting")).x));
                                GUILayout.Space(15); 
                                
                            }
                            GUILayout.EndHorizontal();
                
                             scroll_position = GUILayout.BeginScrollView (scroll_position /*, GUILayout.Width (100), GUILayout.Height (100)*/);
                            {
            
                                foreach (var pin in received_pins)
                                {
                                    drawPinGUI(pin);
                                }                                        
                            }
                            GUILayout.EndScrollView();
                            
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndArea();
                
            }
                
            // ui states to track what states pins *should* be in. 
            // currently fetching actual pin status from firmata 
            // is not implemented so these are lazily updated
            // when the user changes something through the ui
            class PinUI
            {
                public bool reporting_analog;
                public bool reporting_digital;
                public bool test_state;
                public PinMode last_pin_mode = PinMode.OUTPUT;
                public string pwm_value_buffer = "";
                
            }
            
            Dictionary<Arduino.Pin, PinUI> pin_ui_states = new Dictionary<Arduino.Pin, PinUI>();
            
            void drawPinGUI(Arduino.Pin pin)
            {
                GUIStyle green_button = new GUIStyle(GUI.skin.button);
                green_button.normal.textColor = Color.green;
                
                GUIStyle gray_button = new GUIStyle(GUI.skin.button);
                gray_button.normal.textColor = Color.gray;
                        
                bool show_analog_report_button = false;
                bool show_digital_report_button = false;
                                
                if (!pin_ui_states.ContainsKey(pin))
                    pin_ui_states[pin] = new PinUI();
                
                var ui = pin_ui_states[pin];
                
                foreach ( var pc in pin.capabilities)
                {            
                    if (pc.Mode == PinMode.ANALOG)
                        show_analog_report_button = true;
                    
                    if (pc.Mode == PinMode.OUTPUT)
                        show_digital_report_button = true;
                    
                }        
                
                GUILayout.BeginHorizontal();
                
                string label = "";
                
                label = "D"+pin.number.ToString() + ":" + arduino.digitalRead(pin.number).ToString();
                if (pin.analog_number >= 0)
                    label += " A"+pin.analog_number.ToString() + ":" + arduino.analogRead(pin.analog_number).ToString();
                
                GUILayout.Label(label, GUILayout.Width(100));
                
                // write test
                GUILayout.BeginHorizontal(GUILayout.Width(test_column_width));
                
        
                if (ui.last_pin_mode == PinMode.OUTPUT)
                {
                            
                    if (GUILayout.Button(ui.test_state ? "HIGH": "low", ui.test_state ? green_button : gray_button, GUILayout.Width(50))) 
                    {
                        ui.test_state = !ui.test_state;
                        arduino.digitalWrite(pin.number, ui.test_state ? 1 : 0); 
                        
                        // NB: this appears not to interfere input pins for now, but the semantics are not 
                        // well-defined so this could break later if either firmata implementation changes 
                        // to autamatically update the pin mode on a digitalWrite
                    }
                } else if (ui.last_pin_mode == PinMode.PWM || ui.last_pin_mode == PinMode.SERVO)
                {
                    float current_pwm_value; 
                    float.TryParse(ui.pwm_value_buffer, out current_pwm_value);
                    float pwm_value = GUILayout.HorizontalSlider(current_pwm_value, 0, ui.last_pin_mode == PinMode.SERVO ? 180 : 255, GUILayout.Height(21), GUILayout.Width(50));
                    
                    if (pwm_value != current_pwm_value)
                    {
                        arduino.pinMode(pin.number, (int)ui.last_pin_mode);
                        arduino.analogWrite(pin.number, (int)pwm_value);
                        ui.pwm_value_buffer = pwm_value.ToString();
                    }
                            
                    /* Old style: text entry of pwm values--could be useful for some
                    ui.pwm_value_buffer = GUILayout.TextField(ui.pwm_value_buffer, GUILayout.Width(30));
                    int pwm_value;
                    if (GUILayout.Button("!",GUILayout.Width(20)) && int.TryParse(ui.pwm_value_buffer, out pwm_value))
                    {
                        arduino.pinMode(pin.number, (int)PinMode.PWM);
                        arduino.analogWrite(pin.number, pwm_value);
                    }
                    */
                    
                    
                } else
                {
                    GUILayout.Label(""); // workaround unity gui silliness     
                }
                GUILayout.EndHorizontal();
                
                            
                foreach ( var pc in pin.capabilities)
                {
                    
                    if (GUILayout.Button(pc.Mode.ToString(), ui.last_pin_mode==pc.Mode ? green_button : gray_button))
                    {                
                        arduino.pinMode(pin.number, pc.mode);
                        ui.last_pin_mode = pc.Mode;
                        if (pc.Mode == PinMode.ANALOG)
                        {                                        
                            // mirror behavior of StardardFirmata. note that this behavior is different from digital ports, which do require an explicit reportDigital call to enable reporting
                            ui.reporting_analog = true; 
                            //    arduino.reportAnalog(k, (byte)analog_pins_reporting[k]);                    
                        }
                    }
                }        
                

                GUILayout.FlexibleSpace();
                
                if (show_analog_report_button)
                {            
                    if (GUILayout.Button("Analog", ui.reporting_analog ? green_button : gray_button ))
                    {
                        ui.reporting_analog = !ui.reporting_analog;
                        arduino.reportAnalog(pin.analog_number, (byte)(ui.reporting_analog ? 1 : 0));
                    }
                }
                
                if (show_digital_report_button)
                {            
                    if (GUILayout.Button("Digital", ui.reporting_digital ? green_button : gray_button ))
                    {
                        ui.reporting_digital = !ui.reporting_digital;
                        arduino.reportDigital((byte)pin.port, (byte)(ui.reporting_digital ? 1 : 0));
                        
                        
                        foreach (var p in pin_ui_states.Keys)
                        {
                            if (p.port == pin.port)
                            {
                                pin_ui_states[p].reporting_digital = ui.reporting_digital;    
                            }                    
                        }
                    }
                    
                }        
                GUILayout.EndHorizontal();
                        
            }
            
            void OnDestroy()
            {
                
                if (arduino != null)
                    arduino.Disconnect();
                Debug.Log("OnDestroy called");
            }
        
            
            
        }
    }
