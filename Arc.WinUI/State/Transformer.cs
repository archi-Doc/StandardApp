// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Arc.WinUI;

/// <summary>
/// A class for managing window scaling.<br/>
/// 1. Add &lt;Viewbox x ="viewscale" Stretch="None"&gt; at the top level in Window.xaml.<br/>
/// 2. In the constructor of the Window, call Transformer.Register(this).<br/>
/// 3. Change Transformer.DisplayScaling and call Transformer.Refresh().
/// </summary>
public static class Transformer
{
}
