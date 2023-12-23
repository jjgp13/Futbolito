[System.Serializable]
public class ControlMapping
{
    public string xAxis;
    public string yAxis;
    public string shootButton;
    public string attractButton;
    public string wallPassButton;
    public string leftButton;
    public string rightButton;

    public ControlMapping(string x_axis, string y_axis, string shoot_button, string attract_button, string wallPass_Button,string left_button, string right_button)
    {
        xAxis = x_axis;
        yAxis = y_axis;
        shootButton = shoot_button;
        attractButton = attract_button;
        wallPassButton = wallPass_Button;
        leftButton = left_button;
        rightButton = right_button;
    }
}
