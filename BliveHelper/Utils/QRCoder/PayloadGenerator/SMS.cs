using System;

namespace BliveHelper.Utils.QRCoder
{
    public static partial class PayloadGenerator
    {
        /// <summary>
        /// Generates an SMS payload.
        /// </summary>
        public class SMS : Payload
        {
            private readonly string _number, _subject;
            private readonly SMSEncoding _encoding;

            /// <summary>
            /// Creates an SMS payload without text.
            /// </summary>
            /// <param name="number">Receiver phone number.</param>
            /// <param name="encoding">Encoding type.</param>
            public SMS(string number, SMSEncoding encoding = SMSEncoding.SMS)
            {
                _number = number;
                _subject = string.Empty;
                _encoding = encoding;
            }

            /// <summary>
            /// Creates an SMS payload with text (subject).
            /// </summary>
            /// <param name="number">Receiver phone number.</param>
            /// <param name="subject">Text of the SMS.</param>
            /// <param name="encoding">Encoding type.</param>
            public SMS(string number, string subject, SMSEncoding encoding = SMSEncoding.SMS)
            {
                _number = number;
                _subject = subject;
                _encoding = encoding;
            }

            /// <summary>
            /// Returns the SMS payload as a string.
            /// </summary>
            /// <returns>The SMS payload as a string.</returns>
            public override string ToString()
            {
                switch (_encoding)
                {
                    case SMSEncoding.SMS:
                        return $"sms:{_number}{(string.IsNullOrEmpty(_subject) ? string.Empty : $"?body={Uri.EscapeDataString(_subject)}")}";
                    case SMSEncoding.SMS_iOS:
                        return $"sms:{_number}{(string.IsNullOrEmpty(_subject) ? string.Empty : $";body={Uri.EscapeDataString(_subject)}")}";
                    case SMSEncoding.SMSTO:
                        return $"SMSTO:{_number}:{_subject}";
                    default:
                        return string.Empty;
                }
            }

            /// <summary>
            /// Specifies the encoding type for the SMS payload.
            /// </summary>
            public enum SMSEncoding
            {
                /// <summary>
                /// Standard SMS encoding.
                /// </summary>
                SMS,
                /// <summary>
                /// SMSTO encoding.
                /// </summary>
                SMSTO,
                /// <summary>
                /// SMS encoding for iOS.
                /// </summary>
                SMS_iOS
            }
        }
    }
}