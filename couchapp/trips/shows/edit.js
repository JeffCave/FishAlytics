function(doc, req) {  
  var ddoc = this;
  var Mustache = require("lib/mustache");
  var path = require("vendor/couchapp/lib/path").init(req);

  var indexPath = path.list('index','recent-posts',{descending:true, limit:10});
  var feedPath = path.list('index','recent-posts',{descending:true, limit:10, format:"atom"});
  var commentsFeed = path.list('comments','comments',{descending:true, limit:10, format:"atom"});
  
  var data = {
    header : {
      index : indexPath,
      blogName : 'FishAlytics - Couch',
      feedPath : feedPath,
      commentsFeed : commentsFeed
      ,session : 'here' //req.userCtx
    },
    scripts : {},
    pageTitle : doc ? doc.date : "Create a new post",
    assets : path.asset()
  };
  
  if (doc) {
    data.doc = JSON.stringify(doc);
    data.date = new Date(doc.date).toISOString().substring(0,19);
    data.lat = doc.coords.latitude;
    data.long = doc.coords.longtitude;
    data.alt = doc.coords.altitude;
    data.species = doc.species;
    data.notes = doc.notes;
//    data.tags = doc.tags.join(", ");
  } else {
    data.doc = JSON.stringify({
      type : "post",
      format : "markdown"
    });
  }
  
  return Mustache.to_html(ddoc.templates.edit, data, ddoc.templates.partials);
}

