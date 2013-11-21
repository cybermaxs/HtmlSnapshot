var page = require('webpage').create(),
    system = require('system'),
    address;
	
	phantom.outputEncoding = "utf8";

if (system.args.length === 1) {
    console.error('Usage: prerender.js <some URL>');
    phantom.exit(1);
} else {
    address = system.args[1];
    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('FAIL to load the address');
			phantom.exit(2);
        }
		var documentHTML=page.content;
		
        //remove scritps tags
		var matches = documentHTML.match(/<script(?:.*?)>(?:[\S\s]*?)<\/script>/gi);
        for (var i = 0; matches && i < matches.length; i++) {
            documentHTML = documentHTML.replace(matches[i], '');
        }

		system.stdout.write(documentHTML);
        phantom.exit(0);
    });
}
