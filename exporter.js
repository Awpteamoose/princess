// Needs aseprite at least v1.2-beta1
"use strict";
var exec = require('child_process').execSync;
var fs = require('fs');
var settings = JSON.parse(fs.readFileSync('exporter_config.json', 'utf8'));
settings.aseprite = '"' + settings.aseprite + '"'; // Avoiding whitespace in filepaths

for (var sprite_n = 0; sprite_n < settings.sprites.length; sprite_n++)
{
	var sprite = settings.sprites[sprite_n];
	var sprite_path = '"' + sprite + '.ase"'; // Again, avoiding whitespace
	var sprite_real_name = sprite.match(/\\\w+/)[0].replace("\\", "").replace(" ", "_"); // Trying to get rid of subdirectories

	// Output folder to put exported sprites into
	if (!fs.existsSync(settings.output))
		fs.mkdirSync(settings.output)

	// Each sprite gets its own subfolder
	var path = settings.output + "\\" + sprite_real_name;
	if (fs.existsSync(path)) {
		exec("rm -rf " + path);
	}
	fs.mkdirSync(path);

	var tags = exec(settings.aseprite + " -b --list-tags " + sprite_path).toString().split("\r\n"); // Array of sprite tags
	tags.pop(); // Last one is empty

	for (var i = 0; i < tags.length; i++)
	{
		var tag = tags[i];
		console.log("Exporting with tag '" + tag + "'...");
		var target = '"' + settings.output + "\\" + sprite_real_name + "\\" + tag.replace(" ", "_") + '_001.png"'; // Something like "out\princess\Idle_00X.png"
		console.log(settings.aseprite + " -b --frame-tag " + tag + " " + sprite_path + " --save-as " + target);
		console.log(exec(settings.aseprite + " -b --frame-tag " + tag + " " + sprite_path + " --save-as " + target).toString());
	}
}
