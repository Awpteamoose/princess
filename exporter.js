// Needs aseprite at least v1.2-beta1
"use strict";
var exec = require('child_process').execSync;
var fs = require('fs');
var settings = JSON.parse(fs.readFileSync('exporter_config.json', 'utf8'));

if (fs.existsSync(settings.output))
	exec(`rm -rf ${settings.output}`);

settings.sprites.forEach((sprite) => {
	sprite = ('"' + sprite + '"').match(/(?:")(.+)(?:\\)(.+)(?:")/);

	var sprite_folder = sprite[1];
	var sprite_name = sprite[2];
	var out_sprite_folder = `${settings.output}\\${sprite_folder}\\${sprite_name}`;
	console.log(exec(`mkdir "${out_sprite_folder}"`).toString());

	var tags = exec(`${settings.aseprite} -b --list-tags "${sprite_folder}\\${sprite_name}.ase"`).toString().split("\r\n"); // Array of sprite tags
	tags.pop(); // Last one is empty
	console.log(exec(`${settings.aseprite} -b --data "${out_sprite_folder}\\${sprite_name}.json" --format json-array --list-tags "${sprite_folder}\\${sprite_name}.ase"`).toString());

	tags.forEach((tag) => {
		console.log("Exporting with tag '" + tag + "'...");
		var target = `${out_sprite_folder}\\${tag.replace(' ', '_')}_001.png`;
		console.log(     `"${settings.aseprite}" -b --frame-tag ${tag} "${sprite_folder}\\${sprite_name}.ase" --save-as "${target}"`);
		console.log(exec(`"${settings.aseprite}" -b --frame-tag ${tag} "${sprite_folder}\\${sprite_name}.ase" --save-as "${target}"`).toString());
	});
});
