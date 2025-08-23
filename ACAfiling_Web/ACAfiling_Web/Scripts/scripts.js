// CUSTOM SCRIPTS FOR THE APPLICATION
var $ = jQuery;
// document ready event
$(document).ready(function () {

    // set active nav item on page load
    setActiveNavItem();

    // menu click event
    $('.menu-btn').click(function () {
        if ($('#nav-side').hasClass('open')) {
            $('#nav-side').removeClass('open');
            $(this).removeClass('clicked');
        } else {
            $('#nav-side').addClass('open');
            $(this).addClass('clicked');
        }
    });

    // clear sort-param field on ACA page if page is reloaded
    if ($('#sort-param').length) {
        $('#sort-param').val('');
    }

    // login
    $('.navbar-collapse').on('click','#log-in-btn', function () {
        var email = $('#email').val();
        var pword = $('#pword').val();
        var isPersistent = false;
        if ($('#persistent').is(":checked")) {
            isPersistent = true;
        }

        // TODO: all kinds of validation needs to be done here
        loginUser(email, pword, isPersistent);
    });

    // logout
    $('#nav-side').on('click', '#log-out-btn', function () {
        logoutUser();
    });

    // remove error message when user types in login form
    $('.navbar-collapse').on('keyup', '#email, #pword', function () {
        $(this).parent().parent().find('.error').remove();
    });

    // when file is submitted
    $('#air-file-upload').click(function () {
        submitRecords();
    });

    // when a receipt id is entered (manually)
    $('#receipt-id-entry-btn').click(function () {
        addReceiptId();
    });

    // when a status update is checked (manually)

    // 'start over' header btn
    $('body').on('click', '.header-btn', function () {
        location.reload();
    });
    $('body').on('click', '.header-btn a', function (e) {
        e.stopPropagation();
    });

    // tabs functionality
    $('.tab-btns a').click(function () {
        // hide sorting stuff, if this is for submitting records
        if ($(this).html().toLowerCase().indexOf('submit records') > -1) {
            $('#tax-year-select, #sorting-controls').css('opacity', 0);
        } else { // otherwise, make sure it's visible
            $('#tax-year-select, #sorting-controls').css('opacity', 1);
        }

        // switch tabs
        $('.selected').removeClass('selected');
        $(this).addClass('selected').blur(); // blur removes the focus
        // hide all tab content
        $('.tab-content').hide();
        // display content for clicked tab
        var counter = 1;
        var clicked = this;
        $('.tab-btns a').each(function () {
            if (this == clicked) {
                $('.tab-content:nth-child(' + counter + ')').show();
            }
            counter++;
        });
    });

    // submission detail functionality
    $('.tab-content').on('click', 'div.submission', function () {
        // show submission details
        // if the results are already visible, hide them
        var differentDiv = true;
        if ($(this).find('.status-updates').is(':visible')) {
            differentDiv = false;
        }
        // close any open status updates
        $('.status-updates').slideUp();

        // if they're already present, just expand the div
        if (differentDiv && $(this).find('.child-record').length > 0) {
            $(this).find('.status-updates').slideDown();
        } else if (differentDiv) {
            // otherwise, populate results
            getAllUpdatesForSubmission.apply(this);
        }
    });

    // stop 'bubbling up' on all child elements of .submission
    $('.tab-content').on('click', '.submission .status-updates div', function (e) {
        e.stopPropagation();
    });

    // show errors in dashboard
    $('.tab-content').on('click', '.view-error-btn', function (e) {
        e.stopPropagation();
        showErrorsInDashboard.apply(this);
    });

    // get status update from IRS
    $('.tab-content').on('click', '.get-update', function (e) {
        e.stopPropagation(); // prevents click event from bubbling up the DOM tree
        getStatusUpdateFromIrs.apply(this);
    });
    // any clicking on child '.update's should not accordian the list
    $('.tab-content').on('click', '.update', function (e) {
        e.stopPropagation();
    });

    // archive submission on click
    $('#pending-subs').on('click', '.archive-btn', function (e) {
        e.stopPropagation();
        archiveSubmission.apply(this, [true]);
    });
    $('#archived-subs').on('click', '.archive-btn', function (e) {
        e.stopPropagation();
        archiveSubmission.apply(this, [false]);
    });

    // show textbox if 'client' or 'status' is selcted and sort based on selections
    $('#sort-by').change(function () {
        // clear sort-param field
        $('#sort-param').val('');
        // search-terms should be hidden
        $('#search-terms').hide().html('');
        // is this active or archive?
        var showArchive = archiveVisible();
        // did they select a tax year?
        var taxYr = $('#tax-year').val();
        $(this).children().each(function () {
            // 'this' will now refer to the child
            if ($(this).is(":selected")) {
                if ($(this).val() == "Client") {
                    closeStatuses();
                    $('#sort-param').width(0).show().animate({ width: '125' });
                } else if ($(this).val() == "Status") {
                    closeSortParam();
                    $('#statuses').width(0).show().animate({ width: '125' });
                } else if ($(this).val() == "Date") { // sort the submissions
                    closeStatuses();
                    closeSortParam();
                    //sortSubmissionsBy('date', '', showArchive, taxYr); // now we use a sort button
                } else {
                    closeStatuses();
                    closeSortParam();
                }
            }
        });
    });

    // sort
    $('#sort-btn').click(function () {
        // get parameters
        var sortBy = $('#sort-by').val();
        var sortParam = '';
        if ($('#search-terms').is(':visible')) { // if the sortParam is visible, grab it
            sortParam = $('#search-terms').children().first().html().trim();
        }
        var showArchive = archiveVisible();
        var taxYr = $('#tax-year').val();
        sortSubmissionsBy(sortBy, sortParam, showArchive, taxYr);
    });

    // sort by tax-yr (modeled off of the above logic)


    // submission filing:
    // if user clicks radio button that indicates "replacement" or "correction" type of submission
    $('#aca-file-upload #sub-type').change(function () {
        // try to remove the dropdown incase it's there from an earlier click
        $('#correction-specifics').remove();

        var submissionType = $(this).val();
        if (submissionType == "C" || submissionType == "R") {
            getReceiptIdsForCorrOrRepl(submissionType);
        }
    });

    // clear error message if user tries to correct problem
    $('.submit-data').on('click', '#uploadedFile, #sub-type, #receipt-id-list', function () {
        if ($(this).next().hasClass('error')) {
            $(this).next().remove();
        }
    });
    // clear error message if user tries to correct problem
    $('.submit-data').on('focus', '#uploadedFile, #sub-type, #receipt-id-list', function () {
        if ($(this).next().hasClass('error')) {
            $(this).next().remove();
        }
    });

    // if items in the archive, change 'title' attribute for the archive button to "Unarchive"
    $('#archived-subs .submission .archive').each(function () {
        $(this).attr('title', 'Unarchive');
    });

    // sort-param search keypress
    $('#sort-param').keyup(function () {
        var searchTerm = $(this).val();
        // look for search terms
        getSearchTerms(searchTerm);
    });

    // grab click events on client that user wants to sort by
    $('#search-terms').on('click', 'span', function () {
        // clear sort-param field
        $('#sort-param').val('');
        // close #search-terms
        $('#search-terms').hide().html('');

        var clientIdString = $(this).attr('id');
        var clientIdArray = clientIdString.split('_');
        var clientId = clientIdArray[1];
        var showArchive = archiveVisible();
        var taxYr = $('#tax-year').val();

        sortSubmissionsBy('client', clientId, showArchive, taxYr);
    });

    // grab click events on status that user wants to sort by
    $('#statuses').on('click', 'option', function () {
        var status = $(this).val();
        var showArchive = archiveVisible();
        var taxYr = $('#tax-year').val();
        if ($(this).html().trim() != 'select...') {
            sortSubmissionsBy('status', status, showArchive, taxYr);
        }
    });

    // add user (get edit form)
    $('#profile-container').on('click', '.u.heading a', function () {
        // get edit user form
        loadCreateUserForm();
    });

    // edit user (get edit form)
    $('#profile-container').on('click', '#edit-profile', function () {
        var email = $(this).parent().find('.email-address').val();
        loadEditUserForm(email, true);
    });
    $('#user-mgmt').on('click', '.u-controls a', function () {
        var email = $(this).parent().prev().find('.u-email').val();
        // determine if clicked 'edit' or 'delete'
        if ($(this).html().indexOf('Edit') > -1) {
            loadEditUserForm(email, false);
        } else {
            // TODO: CREATE DELETE FUNCTIONALITY
        }
    });


    // manually track submissions
    $('#stat-update-has-errors').attr('checked', false); // make sure it's unchecked on page load
    $('.stat-update-extra-fields input[type="text"]').val(''); // make sure extra fields are empty
    $('#stat-update-has-errors').click(function () {
        if ($(this).is(':checked')) {
            $('.stat-update-extra-fields').slideDown();
        } else {
            $('.stat-update-extra-fields').slideUp();
            $('.stat-update-extra-fields input[type="text"]').val('');
        }
    });


    // add error row to the track submissions form
    $('#status-update-entry').on('click', '.stat-update-error-add-row', function (e) {
        e.preventDefault();
        var nextRowNum = $('.stat-update-extra-fields > .row').length; // this works because there's a heading row that's being counted
        var row = '<div class="row"><div class="col-md-2"><input type="text" id="stat-update-error-code_' + nextRowNum +
            '" name="stat-update-error-code_' + nextRowNum + '" /></div>';
        row += '<div class="col-md-4"><input type="text" id="stat-update-error-msg_' + nextRowNum +
            '" name="stat-update-error-msg_' + nextRowNum + '" /></div>';
        row += '<div class="col-md-3"><input type="text" id="stat-update-xpath_' + nextRowNum +
            '" name="stat-update-xpath_' + nextRowNum + '" /></div>';
        row += '<div class="col-md-2"><input type="text" id="stat-update-error-record-id_' + nextRowNum +
            '" name="stat-update-error-record-id_' + nextRowNum + '" /></div>';
        row += '<div class="col-md-1"><button class="stat-update-error-add-row btn" title="add new row">+</button></div>';
        row += '</div>'; // close .row
        $('.stat-update-extra-fields').append(row);
    });

    // manually track submission status updates
    $('#stat-update-entry-btn').click(function (e) {
        e.preventDefault();
        trackStatusUpdate_Manually();
    });


    // REPORTING FUNCTIONS
    // report selection click event
    $('.page-container').on('click', '.report-link', function () {
        startReportRun.apply(this);
    });

    // run submission status report
    $('.page-container').on('click', '.root-submission', function () {
        runSubmissionStatusReport.apply(this);
        $('#print-btn').removeClass('hidden');
    });

    // run error report from main reports menu
    $('.page-container').on('click', '.submission-with-error', function () {
        runSubmissionErrorReport.apply(this);
    });

    // close lightbox
    $('#close-lightbox').click(function () {
        closeLightBox();
    });

}); // end document.ready()

// submit edit user form
$('#lightbox').on('click focus', 'button', function () {
    // gonna need a bunch of validation checks
    // i don't think it matters if the form is being used for an
    // edit or a new user.  should be able to sort that out at 
    // the db level
    if (validUserEditForm()) {
        submitUserEdits();
    }
});


// clear error messages in profile edit form
$('#lightbox').on('click', '#email, #password1, #password2, #first-name, #last-name, #company, .app-item', function () {
    if ($(this).next().hasClass('error')) {
        $(this).next().remove();
    }
});


// window load event
$(window).load(function () {

});


// window resize event (for responsive stuff)
$(window).resize(function () {

});



// GENERAL FUNCTIONS //

// login user
function loginUser(email, pword, isPersistent) {
    var url = '/UserMgmt/LoginUser';
    var data = { 'email': email, 'pword': pword, 'isPersistent': isPersistent };

    // hide login form and show a loader gif
    $('.navbar-right').animate({ width: 0 }, function () {
        $(this).remove();
        $('.navbar-collapse').append('<ul class="nav navbar-nav navbar-right login"><li><img src="/Images/ellipsis.gif" /></li></ul>');
    });

    $.ajax({
        url: url,
        type: "POST",
        data: data,
        cache: false,
        success: function (result) {
            if (result != null && result.length > 0) {
                // remove loader
                $('.navbar-right').remove();
                // load contents
                $('.navbar-collapse').append(result);
            } else {
                handleFailedLogin();
            }
        },
        error: function (header, textStatus, error) {
            handleFailedLogin();
        }
    }); // end ajax
}

function handleFailedLogin() {
    $('.navbar-right').remove();
    var html = '<ul class="nav navbar-nav navbar-right login"><li><input type="text" id="email" placeholder="email" /></li>' +
                '<li><input type="password" id="pword" placeholder="password" /></li>' +
                '<li><label>Stay logged in?</label><input type="checkbox" id="persistent" /></li>' +
                '<li><a href="javascript:void(0);" id="log-in-btn">Log in</a></li>' +
                '<li class="error" style="position: absolute;top: 25px; line-height: 1.25;">Login failed. Please make sure you ' +
                'are using a valid email / password combination.</li></ul>';
    $('.navbar-collapse').append(html);
}

//  log out user
function logoutUser() {
    var url = '/UserMgmt/LogOutUser';

    // hide login form and show a loader gif
    //$('.navbar-right').animate({ width: 0 }, function () {
    //    $(this).remove();
    //    $('.navbar-collapse').append('<ul class="nav navbar-nav navbar-right"><li><img src="/Images/ellipsis.gif" /></li></ul>');
    //});

    $.ajax({
        url: url,
        type: "GET",
        cache: false,
        success: function (result) {
            // remove loader
            $('.navbar-right').remove();
            // load contents
            //$('.navbar-collapse').append(result);
            var url = window.location.hostname;
            if (url.indexOf('localhost') > -1) {
                url = 'http://localhost:57869/';
            }
            window.location.href = '//ezebshub.com';
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


function archiveVisible() {
    var vis = false;
    if ($('#archived-subs').is(':visible')) {
        vis = true;
    }
    return vis;
}

// submit health records
function submitRecords() {
    if (validSubmissionForm()) {
        var url = '/Home/SubmitData';
        // get file
        var data = new FormData();
        var fileData = $('form#aca-file-upload #uploadedFile').prop('files');
        for (var i = 0; i < fileData.length; i++) {
            data.append("uploadedFile", fileData[i]);
        }
        var otherData = $('form#aca-file-upload').serializeArray();
        $.each(otherData, function (key, input) {
            data.append(input.name, input.value);
        });
        if (fileData != null) {
            // add loader gif
            showLoader();
            $.ajax({
                url: url,
                type: "POST",
                data: data,
                processData: false,
                contentType: false,
                traditional: true,
                //cache: false,
                success: function (result) {
                    //var contents = result;
                    /*if ($.IsXMLDoc(result)) {
                        contents = $.parseXML(result);
                    }*/
                    closeLoader(); // remove loader gif
                    //$('.submit-data').append(result);
                    // all submission functionality will be handled from a WebJob
                    // just let user know that they'll receive an email when it's ready
                    $('.submit-data').html("<h4>Thanks for your submission.  An update on your submission's status will be emailed to you shortly.</h4>");
                },
                error: function (header, textStatus, error) {
                    alert('Error thrown (status: '+ header.status + ' -- ' + textStatus + '): ' + error);
                }
            }); // end ajax
        } // end if fileData
    } // end if valid
} // end submitRecords


// the manual process to add a Receipt ID to an existing function
function addReceiptId() {
    var sub = $('#receiptless-submissions').val();
    var recId = $('#receipt-id').val();
    if (sub !== undefined && recId !== undefined && recId.length > 0) {
        var url = '/Home/AddReceiptId?uid=' + sub + '&receiptId=' + recId;
        
        showLoader();
        $.ajax({
            url: url,
            type: "GET",
            cache: false,
            success: function (result) {
                closeLoader(); // remove loader gif
                $('.receipt-id-entry-content').html("<h4>The Receipt ID has been added to the submission. Thanks!</h4>");
            },
            error: function (header, textStatus, error) {
                var msg = 'Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error;
                $('.receipt-id-entry-content').html('<p>' + msg + '</p>');
            }
        }); // end ajax
    } // end if
} // end addReceiptId


// manually track submission status updates
function trackStatusUpdate_Manually() {
    // will submit via ajax POST
    if ($('#stat-update-rid').val().length > 0 && $('#stat-update-status').val().length > 0) {
        // get form values
        var data = new FormData();
        var otherData = $('form#status-update-entry').serializeArray();
        $.each(otherData, function (key, input) {
            data.append(input.name, input.value);
        });

        var url = '/Home/TrackStatusUpdate_Manual';
        if (data != null && data !== undefined) {
            showLoader();
            $.ajax({
                url: url,
                type: "POST",
                data: data,
                processData: false,
                contentType: false,
                traditional: true,
                //cache: false,
                success: function (result) {
                    //var contents = result;
                    /*if ($.IsXMLDoc(result)) {
                        contents = $.parseXML(result);
                    }*/
                    closeLoader(); // remove loader gif
                    //$('.submit-data').append(result);
                    // all submission functionality will be handled from a WebJob
                    // just let user know that they'll receive an email when it's ready
                    $('.submit-data').html("<h4>Thanks for your submission.  An update on your submission's status will be emailed to you shortly.</h4>");
                },
                error: function (header, textStatus, error) {
                    alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
                }
            }); // end ajax
        }
    }
}


function validSubmissionForm() {
    var valid = true;
    // check the various form inputs and make sure they're valid before submitting

    if ($('#uploadedFile').get(0).files.length === 0) { // no file uploaded
        $('#uploadedFile').after('<div class="error">Please attach an appropriate Excel file.</div>');
        valid = false;
    }
    if ($('#sub-type').val() == 'X') { // they didn't select a submission type
        $('#sub-type').after('<div class="error">Please select the appropriate submission type.</div>');
        valid = false;
    }
    if ($('#receipt-id-list').length && $('#receipt-id-list').val() == 'X') {
        $('#receipt-id-list').after('<div class="error">Please select the Receipt ID of the transmission you\'re replacing.</div>');
        valid = false;
    }

    return valid;
}


function validUserEditForm() {
    // clear any existing errors
    $('.error').remove();

    var valid = true;
    // make sure all inputs are valid before submitting

    // email
    if (($('#email').val().indexOf('@') < 1) || $('#email').val().indexOf('.') < 0) {
        $('#email').after('<div class="error">Please enter a valid email address.</div>');
        valid = false;
    }
    // password
    if ($('#password1').val().length < 6) {
        $('#password1').after('<div class="error">Please enter a password that\'s at least 6 characters.</div>');
        valid = false;
    }
    // password match
    if ($('#password1').val() != $('#password2').val()) {
        $('#password2').after('<div class="error">Passwords don\'t match...</div>');
        valid = false;
    }
    // first name
    if ($('#first-name').val().length < 1) {
        $('#first-name').after('<div class="error">Please enter your first name.</div>');
        valid = false;
    }
    // last name
    if ($('#last-name').val().length < 1) {
        $('#last-name').after('<div class="error">Please enter your last name.</div>');
        valid = false;
    }
    // company
    if ($('#company').val() == "0") {
        $('#company').after('<div class="error">Please select the company this user represents.</div>');
        valid = false;
    }
    // application
    var checked = false;
    $('.app-item').each(function () {
        if ($(this).is(':checked')) {
            checked = true;
        }
    });
    if (!checked) {
        $('#app-label').after('<div class="error">Please select an application for this user.</div>');
        valid = false;
    }

    return valid;
}


// provide dropdown of receipt ids that this submission could be correcting or replacing
function getReceiptIdsForCorrOrRepl(submissionType) {
    // append loader
    $('#aca-file-upload').append('<img id="dot-dot-dot" src="/Images/ellipsis-blk.gif" />');
    // get the available receipt ids
    //var data = { 'subType': submissionType };
    $.ajax({
        url: '/Home/GetReceiptIds?subType=' + submissionType,
        type: "GET",
        //data: data,
        //processData: false,
        contentType: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            $('#dot-dot-dot').remove();
            $('#aca-file-upload').append(result);
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


// pull all updates and child submissions from db
function getAllUpdatesForSubmission() {
    // 'this' refers to the clicked div
    var clickedDiv = this;
    var uid = $(clickedDiv).find('.unique-id').val();
    uid = $.trim(uid);

    // add loading gif to div
    $(clickedDiv).find('.status-updates').append('<div id="loader"><img src="/Images/preloader.gif" /></div>');
    $.ajax({
        url: '/Home/GetUpdatesForSubmission',
        type: "POST",
        data: { 'uid': uid },
        //processData: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            $('#loader').remove(); // remove loader gif
            $(clickedDiv).find('.status-updates').append(result).slideDown();
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


// get status update from IRS
function getStatusUpdateFromIrs() {
    // 'this' refers to the clicked .get-update anchor tag
    var clickedAnchor = this;
    // get receipt id
    var receiptId = $(this).parent().parent().parent().parent().children().first().val();
    receiptId = receiptId.trim();
    if (receiptId.indexOf('\n') > -1) {
        receiptId.replace('\n', '');
    }

    // add loading gif
    $(clickedAnchor).parent().html('<img src="/Images/ellipsis-blk.gif" class="ellipsis-blk"/>');
    //showLoader();

    // get the update
    $.ajax({
        url: '/Home/GetStatusUpdateFromIrs',
        type: "POST",
        data: { 'receiptId': receiptId },
        //processData: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            // the code below contains the original functionality. We're changing it up. Results won't come back from the 
            // server's GetStatusUpdateFromIrs function. Instead, the actual status update will take place in a background
            // process, and the DB will be updated when that finishes. We need to create a function that will ping the DB
            // periodically after the user has asked for a status update for any submission statuses that have the ellipses
            setInterval(checkDbForStatusUpdates, 60000);

            //closeLoader();
            // get contents of returned div
            //var contents = $(result).filter('.update').html();
            //var contents = result;
            // fade the element's contents, replace them, and fade it back in
           /* $(clickedAnchor).parent().parent().animate({ opacity: 0 }, function () {
                $(this).html(contents).animate({ opacity: 1 });
            });*/
        },
        error: function (header, textStatus, error) {
            closeLoader();
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


// check the db for updates to pending statuses
function checkDbForStatusUpdates() {
    $('.ellipsis-blk').each(function () { // this is a loop
        // get the receipt-id
        var recId = $(this).parent().parent().parent().parent().children().first().val();
        var currElem = this;
        // check db for a status other than 'processing'
        $.ajax({
            url: '/Home/GetLatestStatusUpdateByReceiptId',
            type: "POST",
            data: { 'receiptId': recId },
            //processData: false,
            traditional: true,
            //cache: false,
            success: function (result) {
                if (result != null && result.length > 0) {
                    $(currElem).parent().parent().html(result).removeClass('update check-for-new').addClass('stat-up child-text clearfix');
                }
            },
            error: function (header, textStatus, error) {
                closeLoader();
                alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
            }
        }); // end ajax
    }); // end each
}


// show errors in dashboard
function showErrorsInDashboard() {
    var clickedElem = this;
    // only run function if the error list hasn't been pulled already
    if ($(clickedElem).parent().parent().parent().find('.child-errors').children().length < 1) {
        // get receipt id
        var receiptId = $(this).parent().parent().parent().parent().find('.child-receipt-id').val();
        // show some kind of loading gif
        showLoader();
        // get the update
        $.ajax({
            url: '/Home/GetErrorsForSubmission',
            type: "POST",
            data: { 'receiptId': receiptId },
            //processData: false,
            traditional: true,
            //cache: false,
            success: function (result) {
                closeLoader();
                $(clickedElem).parent().parent().parent().find('.child-errors').html(result).slideDown();
            },
            error: function (header, textStatus, error) {
                closeLoader();
                alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
            }
        }); // end ajax
    } // end if
}


function showLoader() {
    $('.body-content').append('<div id="loading"></div>');
}

function closeLoader() {
    $('#loading').remove();
}


// get the search terms related to this item...
function getSearchTerms(searchTerm) {
    // get type of sort user wants to perform
    var sortType;
    $('#sort-by').children().each(function () {
        if ($(this).is(':selected')) {
            sortType = $(this).val();
        }
    });
    // get the items that match under this sortType and searchTerm
    $.ajax({
        url: '/Home/GetSearchTerms',
        type: "POST",
        data: { 'sortType': sortType, 'searchTerm': searchTerm },
        //processData: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            // load
            $('#search-terms').html(result).fadeIn();
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}

// sort the submissions by...
function sortSubmissionsBy(sortType, sortParam, showArchive, taxYr) {
    // add gif
    showLoader();
    // reload submissions
    $.ajax({
        url: '/Home/StatusReporting',
        type: "POST",
        data: { 'sortType': sortType, 'sortParam': sortParam, 'showArchive': showArchive, 'taxYr': taxYr },
        //processData: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            closeLoader();
            // load
            if (!showArchive) { // load results in 'pending' div
                $('#pending-subs').html(result);
            } else { // load results in 'archived' div
                $('#archived-subs').html(result);
            }
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}

function closeSortParam() {
    if ($('#sort-param').is(':visible')) {
        $('#sort-param').animate({ width: 0 }, function () { $(this).hide(); });
        $(this).val('');
    }
}

function closeStatuses() {
    if ($('#statuses').is(':visible')) {
        $('#statuses').animate({ width: 0 }, function () { $(this).hide(); });
    }
}


function loadEditUserForm(email, editingSelf) {
    // get the edit form
    var url = '/UserMgmt/EditUser';

    $.ajax({
        url: url,
        type: "POST",
        data: { 'email': email },
        //processData: false,
        //contentType: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            // load form
            $('.lightbox-content').append(result);
            // show lightbox
            $('#lightbox').fadeIn();
            if (editingSelf) {
                $('.lightbox-content').append('<input type="hidden" id="edit-self" />');
            }
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax

}


function loadCreateUserForm() {
    // get the edit form
    var url = '/UserMgmt/NewUser';

    $.ajax({
        url: url,
        type: "GET",
        cache: false,
        success: function (result) {
            // load form
            $('.lightbox-content').append(result);
            // show lightbox
            $('#lightbox').fadeIn();
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


function closeLightBox() {
    $('#lightbox').fadeOut(function () {
        $('#close-lightbox').next().remove();
    });
}


function submitUserEdits() {
    // get user info
    var email = $('#edit-user-form #email').val();
    var pword = $('#edit-user-form #password1').val();
    var pwordCheck = $('#edit-user-form #password2').val();
    // perform password check
    var fName = $('#edit-user-form #first-name').val();
    var lName = $('#edit-user-form #last-name').val();
    var compId;
    $('#edit-user-form #company option').each(function () {
        if ($(this).is(':selected')) {
            compId = $(this).val();
        }
    });
    var isAdmin = false;
    if ($('#is-admin').is(':checked')) {
        isAdmin = true;
    }
    var apps = [];
    $('.app-item').each(function () {
        if ($(this).is(':checked')) {
            apps.push($(this).val());
        }
    });

    // is user editing self
    var editingSelf = false;
    if ($('#edit-self').length) {
        editingSelf = true;
    }

    // close lightbox
    closeLightBox();

    // display loader gif
    $('#user-mgmt').html('<div id="loader"><img src="/Images/preloader.gif" /></div>');

    // submit
    var url = '/UserMgmt/SubmitUserEdits';
    var data = {
        'email': email, 'pword': pword, 'firstName': fName, 'lastName': lName, 'companyId': compId,
        'isAdmin': isAdmin, 'appSlugs': apps
    };

    $.ajax({
        url: url,
        type: "POST",
        data: data,
        //processData: false,
        //contentType: false,
        traditional: true,
        //cache: false,
        success: function (result) {
            // remove loader gif
            $('#loader').remove();
            // load form
            $('#user-mgmt').html(result);
            // if editing self, reload user's info
            if (editingSelf) {
                reloadUserData(fName, lName);
            }
        },
        error: function (header, textStatus, error) {
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


// reload a user's data
function reloadUserData(fName, lName) {
    $('#profile-info .name').html(fName + " " + lName);
    $('.nav .welcome').html('Hi, ' + fName);
}


// REPORTING FUNCTIONS
function startReportRun() {
    showLoader();
    // 'this' refers to the clicked report item
    // get the particular report that needs to be run
    var report = $(this).attr('report-function');
    var url = '/Reporting/' + report;

    $.ajax({
        url: url,
        type: "GET",
        cache: false,
        success: function (result) {
            // change header
            changeHeaderToStartOverBtn();
            changePageContainerContents(result);
            closeLoader();
        },
        error: function (header, textStatus, error) {
            closeLoader();
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}

// change header to 'start over' button
function changeHeaderToStartOverBtn() {
    //$('.body-content h1').html('<< Start Over').addClass('header-btn');
    $('.body-content h1').html('<< Start Over<a id="print-btn" class="hidden" href="javascript:window.print()">Print</a>').addClass('header-btn');
}

// change out content of 'page-container'
function changePageContainerContents(results) {
    // set min-height of page-container so it doesn't collapse
    var currHeight = $('.page-container').height();
    $('.page-container').css('min-height', currHeight);

    // fade out current contents
    $('.page-container div').fadeOut(function () {
        // replace content and fade back in
        $('.page-container').html(results).fadeIn();
    });
}

//
function runSubmissionStatusReport() {
    showLoader();
    // get receipt id
    var receiptId = $(this).children().first().next().html();
    receiptId = receiptId.replace('\n', '');
    receiptId = receiptId.trim();
    // get the report
    var url = '/Reporting/GetSubmissionStatusReport?receiptId=' + receiptId;

    $.ajax({
        url: url,
        type: "GET",
        cache: false,
        success: function (result) {
            changePageContainerContents(result);
            closeLoader();
        },
        error: function (header, textStatus, error) {
            closeLoader();
            alert('Error thrown (status: ' + header.status + ' -- ' + textStatus + '): ' + error);
        }
    }); // end ajax
}


function runSubmissionErrorReport() {
    //showLoader();
    // get receipt id
    var receiptId = $(this).children().first().next().html();
    receiptId = receiptId.replace('\n', '');
    receiptId = receiptId.trim();
    // get the report
    var url = '/Reporting/RunErrorReport?receiptId=' + receiptId;
    openInNewTab(url);
}


function openInNewTab(url) {
    var win = window.open(url, '_blank');
    if (win) {
        // success!
        win.focus();
    } else {
        alert("Report was blocked. Please allow popus for this site.");
    }
}


// archive a submission
function archiveSubmission(isArchived) {
    var receiptId = $(this).parent().find('.receipt-id').html();
    receiptId = receiptId.replace('\n', '');
    receiptId = receiptId.trim();
    var clickedDiv = this;
    // archive it
    var url = '/Home/ArchiveSubmission?receiptId=' + receiptId + '&isArchived=' + isArchived;
    $.ajax({
        url: url,
        type: "GET",
        cache: false
    }); // end ajax
    // remove the submission
    $(clickedDiv).parent().parent().fadeOut(function () {
        var submission = $(this).detach();
        // are there already archives listed?
        if ($('#archived-subs #submission-list .heading').length) {
            $('#archived-subs #submission-list .heading').after(submission);
        } else {
            // no heading... create one
            var heading = $('#pending-subs .heading').outerHtml();
            // insert the elements
            $('#archived-subs #submission-list').append(heading).append(submission);
        }
    });
    // reload archive submissions
    //$('#archived-subs').load('/Home/StatusReporting', { sortType: "", sortParam: "", showArchive: true });
}

function setActiveNavItem() {
    $('.active-link').removeClass('active-link');
    var url = window.location.href;
    $('#nav-side li a').each(function () {
        if (url.indexOf($(this).attr('href')) > -1) {
            $(this).parent().addClass('active-link');
        }
    });
    // if 2 items received the 'active-link' class, remove the first one (since it's the home page)
    if ($('.active-link').length > 1) {
        $('.active-link').first().removeClass('active-link');
    }
}
