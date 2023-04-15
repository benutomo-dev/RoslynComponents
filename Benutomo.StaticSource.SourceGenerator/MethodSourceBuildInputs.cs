﻿using Microsoft.CodeAnalysis.Text;

namespace Benutomo.AutomaticNotifyPropertyChangedImpl.SourceGenerator
{
    record MethodSourceBuildInputs(SourceText SourceText, string Namespace, string TypeName, bool UseSelfBuilding);
}
