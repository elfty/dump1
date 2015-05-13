using System;
using System.Collections.Generic;
using System.Text;

namespace YJ.AppLink.Api
{
    public class ParticipantData
    {
        private Participant participant;

        internal ParticipantData(Participant p)
        {
            this.participant = p;
        }

        public string Name
        {
            get { return this.participant.Name; }
        }

        public ParticipantType ParticipantType
        {
            get
            {
                if (string.IsNullOrEmpty(participant.Type))
                    return Api.ParticipantType.Unknown;

                switch (participant.Type)
                {
                    case "Contact":
                        return Api.ParticipantType.Contact;
                    case "NonContact":
                        return Api.ParticipantType.NonContact;
                    case "User":
                        return Api.ParticipantType.User;
                    case "Desk":
                        return Api.ParticipantType.Desk;
                    case "Coworker":
                        return Api.ParticipantType.Coworker;
                    case "Unknown":
                    default:
                        return Api.ParticipantType.Unknown;
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            ParticipantData pd = obj as ParticipantData;
            return (pd != null && Name.Equals(pd.Name) && ParticipantType == pd.ParticipantType);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + ParticipantType.GetHashCode();
        }

    }

    public enum ParticipantType
    {
        Contact,
        NonContact,
        User,
        Coworker,
        Desk,
        Unknown
    }
}
