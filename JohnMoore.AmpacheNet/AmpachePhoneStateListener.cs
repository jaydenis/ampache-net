//
// ConfigurationActivity.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2011 John Moore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	public class AmpachePhoneStateListener : Android.Telephony.PhoneStateListener
	{
		private readonly AmpacheModel _model;
		private bool _resumeOnIdle = false;
		
		public AmpachePhoneStateListener (AmpacheModel model)
		{
			_model = model;
		}
		
		public override void OnCallStateChanged (Android.Telephony.CallState state, string incomingNumber)
		{
			base.OnCallStateChanged (state, incomingNumber);
			if(state == Android.Telephony.CallState.Idle && _resumeOnIdle)
			{
				_model.PlayPauseRequested = true;
				_resumeOnIdle = false;
			}
			else if(state == Android.Telephony.CallState.Ringing && _model.IsPlaying)
			{
				_model.PlayPauseRequested = true;
				_resumeOnIdle = true;
			}
		}
	}
}

