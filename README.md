HtmlSnapshot
============

HtmlSnapshot is a proof-of-concept for generating static Html (scriptless) content on IIS/asp.net

Run the demo
--------------
Run one of the samples and change your browser User Agent (F12 Developper Tools in Chrome/IE). 
All pages will be rendered as Html without any script tag and a header is added at the top of the page.


How it works ?
--------------
At startup a dynamic HttpModule is registered. 
For each appropriate request (document only, no script/style, no bundle), the module renders the page using [phantomjs](http://phantomjs.org/) and return the html to the browser.
