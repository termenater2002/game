using System;

namespace Unity.Muse.Common
{
    enum PreferenceScope
    {
        User,           // Actually `Editor-wide` preference, not per cloud user.
        Project,
        Session
    }
}
