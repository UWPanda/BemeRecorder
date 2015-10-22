![picture Header](https://raw.githubusercontent.com/UWPanda/BemeRecorder/master/Images/GitHub_Header.png "Header")

#BemeRecorder
Similar to the Beme app on iOS BemeRecorder uses the ProximitySensor of the device to trigger video recording in UWP.

The start and stop of the video recording is controlled within this method - it's called every time the reading of the ProximitySensor has changed.
```csharp
//Get readings from ProximitySensor
 private async void Sensor_ReadingChanged(ProximitySensor sender, ProximitySensorReadingChangedEventArgs e)
{
	
	ProximitySensorReading reading = e.Reading;
	if (null != reading)
	{
		if (reading.IsDetected)
		{
	
		}
	}
}
```

Once a recording is started the video is stored on the file system in the photos directory. Maybe stitching multiple clips together is comming in a tutorial in the future - this one is just about triggering recordings via the ProxmitySensor.
```csharp
	var videoFile = await KnownFolders.PicturesLibrary.CreateFileAsync("Beme.mp4", CreationCollisionOption.GenerateUniqueName);

	var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
	await mediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);
```

#Screenshot
![picture screenshot](https://raw.githubusercontent.com/UWPanda/TabbedPivot/master/Images/Screenshot.png "Screenshot")

#Author

[nor0x](https://github.com/nor0x) for [UWPanda](https://uwpanda.azurewebsites.net)

#License

BemeRecorder is available under the MIT license. See the LICENSE file for more info.

