function(doc) {
    if (doc._id.substring(0,4) === 'trip') {
        emit(doc.user, doc);
    }
};
