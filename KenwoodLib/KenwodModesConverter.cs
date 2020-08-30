using System;
using System.Collections.Generic;
using System.Text;
using static KenwoodLib.KenwoodTypes;

namespace KenwoodLib
{
  public static class KenwodModesConverter
  {
    public enum ModeValues
    {
      /// <summary> Defines the LSB
      /// </summary>
      LSB = 1,
      /// <summary> Defines the USB
      /// </summary>
      USB = 2,
      /// <summary> Defines the CW
      /// </summary>
      CW = 3,
      /// <summary> Defines the FM
      /// </summary>
      FM = 4,
      /// <summary> Defines the AM
      /// </summary>
      AM = 5,
      /// <summary> Defines the FSK
      /// </summary>
      FSK = 6,
      /// <summary>// Defines the CWR
      /// </summary>
      CWR = 7,
      /// <summary> Defines the Tune
      /// </summary>
      Tune = 8,
      /// <summary> Defines the FSR
      /// </summary>
      FSKR = 9,
      /// <summary> Defines the ERROR
      /// </summary>
      ERROR = 10
    }
    public static ModeValues ModeStandardToKenwoodEnum(string mode)
    {
      if (mode == null) return ModeValues.ERROR;

      switch (mode.ToUpper())
      {
        case "USB":
          return ModeValues.USB;
        case "LSB":
          return ModeValues.LSB;
        case "CW":
          return ModeValues.CW;
        case "CWL":
          return ModeValues.CW;
        case "CWU":
          return ModeValues.CW;
        case "AM":
          return ModeValues.AM;
        case "FM":
          return ModeValues.FM;
        case "FSK":
          return ModeValues.FSK;
        case "DIGH":
          return ModeValues.FSK;
        case "DIGL":
          return ModeValues.FSKR;
        case "CWR":
          return ModeValues.CWR;
        case "FSR":
          return ModeValues.FSKR;
        case "TUNE":
          return ModeValues.Tune;
      }
      return ModeValues.ERROR;
    }
  }
}
